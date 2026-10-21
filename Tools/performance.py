#!/usr/bin/env python3
"""Run the opt-in Release macOS player or inspect a copied device performance report."""
import argparse
import json
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]


def inspect(directory):
    try:
        report = json.loads((directory / "report.json").read_text())
        result = (directory / "result.txt").read_text()
    except (OSError, ValueError) as error:
        print(f"FAIL: no readable completed report in {directory}: {error}")
        return 1
    valid = (result.startswith("PASS ") and report["completed"] and report["stabilityPassed"]
             and report["measuredSeconds"] >= report["requestedSeconds"] and report["frameCount"] > 0)
    print(result.strip())
    print(f"Measured {report['measuredSeconds']:.2f}s; development={report['developmentBuild']}; "
          f"FPS {report['averageFps']:.2f}; p95 {report['p95Ms']:.2f}ms; p99 {report['p99Ms']:.2f}ms; "
          f"worst {report['worstMs']:.2f}ms; frames >50ms {report['framesOver50Ms']}")
    resident = f"{report['residentPeakBytes'] / 1048576:.1f}MiB" if report['residentPeakBytes'] >= 0 else "unavailable"
    print(f"Hearts {report['heartsCollected']}; captures {report['captures']}; rounds {report['rounds']}; "
          f"items {report['itemsUsed']}; peak sampled resident {resident}")
    print("Stability completion and frame-budget acceptance are separate. See report.json and samples.csv.")
    return 0 if valid else 1


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    mac = commands.add_parser("mac")
    mac.add_argument("--seconds", type=int, default=900, choices=range(10, 3601), metavar="10..3600")
    mac.add_argument("--warmup", type=int, default=15, choices=range(1, 121), metavar="1..120")
    read = commands.add_parser("report")
    read.add_argument("directory", type=Path)
    args = parser.parse_args()
    if args.command == "report":
        return inspect(args.directory)
    player = ROOT / "Builds/macOS-Performance/Maze of Egos.app/Contents/MacOS/Maze of Egos"
    if not player.is_file():
        parser.error("Build first: python3 Tools/unity.py build-mac-performance")
    output = ROOT / ".artifacts/performance-mac"
    output.mkdir(parents=True, exist_ok=True)
    for name in ("report.json", "result.txt", "samples.csv", "checkpoint.json"):
        (output / name).unlink(missing_ok=True)
    env = os.environ.copy()
    env.update(MAZE_PERFORMANCE_TEST="1", MAZE_PERFORMANCE_SECONDS=str(args.seconds),
               MAZE_PERFORMANCE_WARMUP_SECONDS=str(args.warmup), MAZE_PERFORMANCE_OUTPUT=str(output))
    env.pop("MAZE_SMOKE_TEST", None)
    with (ROOT / "Logs/performance-mac-console.log").open("w") as console:
        try:
            subprocess.run([str(player), "-screen-fullscreen", "0", "-screen-width", "1600", "-screen-height", "900",
                            "-logFile", str(ROOT / "Logs/performance-mac.log")], env=env, cwd=ROOT,
                           stdout=console, stderr=subprocess.STDOUT, timeout=args.seconds + args.warmup + 60, check=False)
        except subprocess.TimeoutExpired:
            print("FAIL: performance player timed out. See Logs/performance-mac.log and the latest checkpoint.")
            return 1
    return inspect(output)


if __name__ == "__main__":
    raise SystemExit(main())
