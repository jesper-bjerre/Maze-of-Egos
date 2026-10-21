#!/usr/bin/env python3
"""Validate portable project assets without starting Unity."""
from pathlib import Path
import hashlib
import json
import re
import sys

ROOT = Path(__file__).resolve().parents[1]


def main():
    specification = (ROOT / "docs/Labyrint_3D_Spildesign_og_Codex_specifikation.md").read_text()
    reference = next(json.loads(block) for block in re.findall(r"```json\n(.*?)\n```", specification, re.S) if '"classic_reference_v1"' in block)
    assert reference == json.loads((ROOT / "Assets/Resources/Data/classic_reference.json").read_text()), "Maze changed relative to source specification"
    assets = json.loads((ROOT / "docs/CHARACTER_ASSETS.json").read_text())
    assert {a["id"] for a in assets} == {"zelenskyy", "putin", "xi", "kim", "trump"}
    for asset in assets:
        source = (ROOT / asset["source"]).read_bytes()
        runtime = (ROOT / asset["runtimeCopy"]).read_bytes()
        assert source == runtime and hashlib.sha256(source).hexdigest() == asset["sha256"], asset["id"]
    pilot = json.loads((ROOT / "art/characters/putin/manifest.json").read_text())
    assert hashlib.sha256((ROOT / pilot["sourceReference"]).read_bytes()).hexdigest() == pilot["sourceSha256"], "Pilot source reference changed"
    assert hashlib.sha256((ROOT / pilot["runtimeModel"]).read_bytes()).hexdigest() == pilot["runtimeSha256"], "Pilot FBX differs from its manifest"
    assert (ROOT / pilot["editableSource"]).is_file(), "Editable Blender source missing"
    guids = {}
    for path in (ROOT / "Assets").rglob("*"):
        if path.suffix == ".meta":
            match = re.search(r"^guid: ([a-f0-9]{32})$", path.read_text(), re.M)
            assert match, f"Invalid metadata: {path}"
            assert match[1] not in guids, f"Duplicate GUID: {path}, {guids.get(match[1])}"
            guids[match[1]] = path
        else:
            assert Path(str(path) + ".meta").is_file(), f"Missing metadata: {path}"
    for path in list((ROOT / "Assets").rglob("*.asmdef")) + [ROOT / "Packages/manifest.json"]:
        json.loads(path.read_text())
    expected = {"MazeOfEgos.Core", "Unity.InputSystem", "UnityEngine.UI", "Unity.RenderPipelines.Universal.Runtime", "Unity.RenderPipelines.Core.Runtime"}
    runtime = json.loads((ROOT / "Assets/Scripts/Runtime/MazeOfEgos.Runtime.asmdef").read_text())
    assert set(runtime["references"]) == expected, "Assembly references do not match the pinned Unity packages"
    print(f"PASS source maze equality, five exact reference copies, pilot source/FBX hashes and editable source, {len(guids)} unique Unity GUIDs, package/assembly JSON and references.")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (AssertionError, OSError, ValueError) as error:
        print(f"FAIL {error}", file=sys.stderr)
        sys.exit(1)
