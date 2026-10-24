#!/usr/bin/env python3
"""Run opted-in checks on an already installed, signed physical iOS build."""
import argparse
import json
from pathlib import Path
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
BUNDLE = 'dk.mazeofegos.game'

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('mode', choices=['smoke', 'lifecycle', 'performance'])
    parser.add_argument('--device', required=True, help='devicectl identifier or device name')
    parser.add_argument('--seconds', type=int, default=900)
    parser.add_argument('--output', type=Path)
    parser.add_argument('--label', default='build12', help='Evidence/log label, e.g. build12')
    args = parser.parse_args()
    if not 10 <= args.seconds <= 3600: parser.error('--seconds must be 10..3600')
    if not args.label.replace('-', '').replace('_', '').isalnum(): parser.error('--label must be alphanumeric with hyphens/underscores')
    output = args.output or ROOT / '.artifacts' / (args.mode + '-iphone-' + args.label)
    if output.exists(): parser.error('Choose a fresh output directory; earlier evidence is preserved.')
    output.parent.mkdir(parents=True, exist_ok=True)
    log = ROOT / 'Logs' / ('iphone-' + args.mode + '-' + args.label + '.log')
    flags = {'smoke': {'MAZE_SMOKE_TEST': '1'}, 'lifecycle': {'MAZE_LIFECYCLE_TEST': '1'},
             'performance': {'MAZE_PERFORMANCE_TEST': '1', 'MAZE_PERFORMANCE_SECONDS': str(args.seconds), 'MAZE_PERFORMANCE_WARMUP_SECONDS': '15'}}[args.mode]
    command = ['xcrun', 'devicectl', 'device', 'process', 'launch', '--device', args.device,
               '--terminate-existing', '--environment-variables', json.dumps(flags), '--console', BUNDLE]
    with log.open('w') as console:
        process = subprocess.Popen(command, cwd=ROOT, stdout=console, stderr=subprocess.STDOUT)
        deadline = time.monotonic() + (args.seconds + 120 if args.mode == 'performance' else 180)
        ready = False; next_progress = 0
        try:
            while process.poll() is None:
                content = log.read_text(errors='replace')
                if args.mode == 'lifecycle' and not ready and 'LIFECYCLE READY' in content:
                    ready = True
                    subprocess.run(['xcrun', 'devicectl', 'device', 'process', 'launch', '--device', args.device, 'com.apple.Preferences'], check=True)
                    time.sleep(8)
                    subprocess.run(['xcrun', 'devicectl', 'device', 'process', 'launch', '--device', args.device, BUNDLE], check=True)
                if time.monotonic() >= deadline: raise TimeoutError('Device test timed out; inspect ' + str(log))
                if args.mode == 'performance' and time.monotonic() > next_progress:
                    lines = [line for line in content.splitlines() if 'PERFORMANCE ' in line]
                    if lines: print(lines[-1], flush=True)
                    next_progress = time.monotonic() + 30
                time.sleep(.5)
        finally:
            if process.poll() is None: process.terminate(); process.wait(timeout=10)
    source = {'smoke': 'maze-smoke-captures', 'lifecycle': 'maze-lifecycle', 'performance': 'maze-performance'}[args.mode]
    subprocess.run(['xcrun', 'devicectl', 'device', 'copy', 'from', '--device', args.device,
                    '--domain-type', 'appDataContainer', '--domain-identifier', BUNDLE,
                    '--source', 'Library/Caches/' + source, '--destination', str(output), '--timeout', '30'], check=True)
    result = (output / 'result.txt').read_text()
    token = {'smoke': 'RUNTIME SMOKE PASSED', 'lifecycle': 'LIFECYCLE PASSED', 'performance': 'PERFORMANCE COMPLETED'}[args.mode]
    # devicectl may return zero even when the app reports a failed check.
    if not result.startswith('PASS ') or token not in log.read_text(errors='replace'):
        raise RuntimeError(result + '\nInspect ' + str(log))
    if args.mode == 'performance':
        report = json.loads((output / 'report.json').read_text())
        if not (report['completed'] and report['stabilityPassed'] and not report['developmentBuild'] and report['measuredSeconds'] >= args.seconds):
            raise RuntimeError('Incomplete or non-Release performance evidence')
    if args.mode == 'lifecycle' and not json.loads((output / 'report.json').read_text())['passed']:
        raise RuntimeError('Lifecycle assertions failed')
    print(result.strip()); print('Evidence: ' + str(output))

if __name__ == '__main__': main()
