#!/usr/bin/env python3
"""Portable Unity commands; no credentials or machine-specific paths are committed."""
import argparse
import os
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
VERSION = (ROOT / "ProjectSettings/ProjectVersion.txt").read_text().splitlines()[0].split(": ", 1)[1]


def smoke_mac():
    player = ROOT / "Builds/macOS/Maze of Egos.app/Contents/MacOS/Maze of Egos"
    if not player.is_file():
        raise SystemExit("Build the development macOS player first: python3 Tools/unity.py build-mac")
    output = ROOT / ".artifacts/smoke-mac"; output.mkdir(parents=True, exist_ok=True)
    logs = ROOT / "Logs"; logs.mkdir(exist_ok=True)
    for capture in output.glob("*.png"):
        capture.unlink()
    (output / "result.txt").unlink(missing_ok=True)
    command = [str(player), "-screen-fullscreen", "0", "-screen-width", "1600", "-screen-height", "900",
               "-logFile", str(logs / "runtime-smoke-mac.log"), "--maze-smoke-test", "--maze-smoke-output", str(output)]
    with (logs / "runtime-smoke-console.log").open("w") as console:
        try:
            result = subprocess.run(command, cwd=ROOT, stdout=console, stderr=subprocess.STDOUT, timeout=90)
        except subprocess.TimeoutExpired:
            print("FAIL: native runtime check timed out. See Logs/runtime-smoke-mac.log")
            return 1
    report = output / "result.txt"
    message = report.read_text() if report.is_file() else "FAIL: player exited without a runtime result."
    print(message.strip()); print("Captures: .artifacts/smoke-mac; log: Logs/runtime-smoke-mac.log")
    return 0 if result.returncode == 0 and message.startswith("PASS ") else 1


def editor_path(explicit=None):
    candidates = []
    if explicit or os.environ.get("MAZE_UNITY_EDITOR"):
        candidates.append(Path(explicit or os.environ["MAZE_UNITY_EDITOR"]).expanduser())
    candidates += [
        Path("/Applications/Unity/Hub/Editor") / VERSION / "Unity.app/Contents/MacOS/Unity",
        Path.home() / "Applications/Unity/Hub/Editor" / VERSION / "Unity.app/Contents/MacOS/Unity",
        Path(os.environ.get("ProgramFiles", "C:/Program Files")) / "Unity/Hub/Editor" / VERSION / "Editor/Unity.exe",
        Path.home() / "Unity/Hub/Editor" / VERSION / "Editor/Unity",
        ROOT / ".tools/unity-expanded/Unity.pkg.tmp/Payload/Unity/Unity.app/Contents/MacOS/Unity",
    ]
    for candidate in candidates:
        if candidate.suffix == ".app":
            candidate = candidate / "Contents/MacOS/Unity"
        if candidate.is_file():
            return candidate.resolve()
    raise SystemExit(f"Unity {VERSION} not found. Install the pinned editor in Unity Hub or pass --editor /path/to/Unity.")


def managed_root(editor):
    candidates = [editor.parent.parent, editor.parent / "Data", editor.parent]
    return next((p for p in candidates if (p / "Resources/Scripting/Managed/UnityEngine/UnityEngine.CoreModule.dll").is_file()), None)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=["configure", "validate", "build-mac", "build-windows", "build-ios", "smoke-mac", "compile-check", "serialization-check"])
    parser.add_argument("--editor", help="Unity executable or macOS .app")
    args = parser.parse_args()
    if args.command == "smoke-mac":
        return smoke_mac()
    editor = editor_path(args.editor)
    if args.command in ("compile-check", "serialization-check"):
        managed = managed_root(editor)
        if managed is None:
            raise SystemExit("The editor's managed library directory could not be located.")
        if args.command == "compile-check":
            command = ["dotnet", "build", "Tools/UnityCompileCheck.csproj", "--nologo", "-v:q", f"-p:UnityEditorRoot={managed}"]
        else:
            command = ["dotnet", "run", "--project", "Tests/MazeOfEgos.SerializationChecks.csproj", f"-p:UnityEditorRoot={managed}"]
        return subprocess.call(command, cwd=ROOT)
    methods = {"configure": "Configure", "validate": "ConfigureAndValidate", "build-mac": "BuildMac", "build-windows": "BuildWindows", "build-ios": "BuildIOS"}
    logs = ROOT / "Logs"; logs.mkdir(exist_ok=True)
    log = logs / f"unity-{args.command}.log"
    command = [str(editor), "-batchmode", "-nographics", "-quit", "-projectPath", str(ROOT),
               "-executeMethod", "MazeOfEgos.Editor.ProjectSetup." + methods[args.command], "-logFile", str(log)]
    if args.command == "build-ios":
        command += ["-buildTarget", "iOS"]
    result = subprocess.call(command, cwd=ROOT)
    print(f"Unity exited with {result}. Log: {log.relative_to(ROOT)}")
    if log.exists():
        relevant = [line for line in log.read_text(errors="replace").splitlines() if any(word in line.lower() for word in ("error", "exception", "validation passed", "build succeeded", "no valid unity", "please activate"))]
        for line in relevant[-15:]: print(line)
    return result


if __name__ == "__main__":
    sys.exit(main())
