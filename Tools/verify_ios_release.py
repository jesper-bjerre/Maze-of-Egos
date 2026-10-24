#!/usr/bin/env python3
"""Inspect a local signed archive; never exports or uploads to App Store Connect."""
import argparse
import hashlib
import json
from pathlib import Path
import plistlib
import struct
import subprocess

ROOT = Path(__file__).resolve().parents[1]
def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--version', default='0.2.0')
    parser.add_argument('--build', default='14')
    parser.add_argument('--archive', type=Path)
    parser.add_argument('--project', type=Path, default=ROOT / 'Builds/iOS-Release')
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    args.archive = args.archive or ROOT / f'Builds/Archives/MazeOfEgos-{args.version}-{args.build}.xcarchive'
    args.output = args.output or ROOT / f'.artifacts/release-audit-build{args.build}.json'
    app = args.archive / 'Products/Applications/MazeofEgos.app'
    info = plistlib.loads((app / 'Info.plist').read_bytes())
    assert info['CFBundleIdentifier'] == 'dk.mazeofegos.game'
    assert info['CFBundleShortVersionString'] == args.version and info['CFBundleVersion'] == args.build
    assert info['UIDeviceFamily'] == [1, 2] and info['MinimumOSVersion'] == '17.0'
    assert info['CFBundleDevelopmentRegion'] == 'en' and info['CFBundleLocalizations'] == ['en']
    orientations = info['UISupportedInterfaceOrientations']
    assert set(orientations) == {'UIInterfaceOrientationLandscapeLeft', 'UIInterfaceOrientationLandscapeRight'}
    permissions = [key for key in info if key.endswith('UsageDescription')]
    assert not permissions, 'Unexpected permission prompts: ' + str(permissions)
    icon = args.project / 'Unity-iPhone/Images.xcassets/AppIcon.appiconset/Icon-Store-1024.png'
    assert struct.unpack('>IIBB', icon.read_bytes()[16:26]) == (1024, 1024, 8, 2), 'Store icon must be RGB without alpha'
    forbidden = ('DevelopmentSmoke', 'DevelopmentLifecycle', 'DevelopmentPerformance', 'CompetitionAutomation')
    generated = list((args.project / 'Il2CppOutputProject/Source/il2cppOutput').glob('MazeOfEgos.Runtime*.cpp'))
    assert generated, 'No generated game assembly found'
    assert all(not any(name in path.read_text() for name in forbidden) for path in generated), 'Test harness leaked into normal Release'
    privacy_files = sorted(app.rglob('PrivacyInfo.xcprivacy'))
    assert privacy_files, 'No bundled privacy manifest'
    reasons = {}
    for path in privacy_files:
        data = plistlib.loads(path.read_bytes())
        assert not data.get('NSPrivacyTracking', False)
        assert not data.get('NSPrivacyCollectedDataTypes', [])
        for entry in data.get('NSPrivacyAccessedAPITypes', []):
            reasons[entry['NSPrivacyAccessedAPIType']] = entry['NSPrivacyAccessedAPITypeReasons']
    for name in ['SystemBootTime', 'DiskSpace', 'UserDefaults', 'FileTimestamp']:
        assert 'NSPrivacyAccessedAPICategory' + name in reasons
    subprocess.run(['codesign', '--verify', '--deep', '--strict', str(app)], check=True)
    executable = app / info['CFBundleExecutable']
    report = dict(passed=True, version=info['CFBundleShortVersionString'], build=info['CFBundleVersion'],
                  bundleId=info['CFBundleIdentifier'], minimumOS=info['MinimumOSVersion'], deviceFamily=info['UIDeviceFamily'],
                  localizations=info['CFBundleLocalizations'], sdk=info.get('DTSDKName'), xcodeBuild=info.get('DTXcodeBuild'), orientations=orientations,
                  usageDescriptionKeys=permissions, storeIcon='1024x1024 RGB8 without alpha',
                  testHarnessAbsent=True, signatureVerified=True, executableSha256=hashlib.sha256(executable.read_bytes()).hexdigest(),
                  requiredReasonAPIs=reasons, manifestDeclaresTracking=False, manifestCollectedDataTypes=[],
                  limitations=['Local development-signed archive, not App Store validation or distribution export.',
                               'Manifest/configuration audit is not a packet capture or legal/rights approval.'])
    args.output.parent.mkdir(parents=True, exist_ok=True); args.output.write_text(json.dumps(report, indent=2) + '\n')
    print('PASS signed local iPhone/iPad archive, version, orientations, permission keys, RGB store icon, privacy manifests and absence of test harness.')
    print(args.output)
if __name__ == '__main__': main()
