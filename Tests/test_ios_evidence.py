"""Completion guards: never accept a stale or partially written device report."""
import copy
from datetime import datetime, timezone, timedelta
import json
from pathlib import Path
import sys
import tempfile
import unittest
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'Tools'))
from performance import validate_competition_report

class DeviceEvidenceChecks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.directory = Path(self.temp.name)
        self.launch = datetime.now(timezone.utc) - timedelta(seconds=30)
        self.report = dict(schemaVersion=2, model='competition-v2', completed=True,
                           stabilityPassed=True, error=None, developmentBuild=False,
                           backgroundInterruptions=0, requestedSeconds=20, measuredSeconds=20.01,
                           frameCount=1200, averageFps=59.97, playingSeconds=18,
                           countdownSeconds=1, resultsSeconds=1.01, usesByActor=[1, 2, 3, 4, 5],
                           utc=datetime.now(timezone.utc).strftime('%Y-%m-%dT%H:%M:%S') + '.1234567Z',
                           samples=[dict(elapsedSeconds=x) for x in [0.01, 5, 10, 15, 20.01]])
        (self.directory / 'result.txt').write_text('PASS native completed recording')
    def tearDown(self):
        self.temp.cleanup()
    def check(self, report=None):
        (self.directory / 'report.json').write_text(json.dumps(report or self.report))
        return validate_competition_report(self.directory, 20, self.launch)
    def test_durable_complete_report_does_not_need_a_console_token(self):
        for model in ('competition-v2', 'competition-v3', 'competition-v4'):
            self.report['model'] = model
            self.assertTrue(self.check()['completed'])
    def test_native_failure_is_not_masked_by_complete_json(self):
        (self.directory / 'result.txt').write_text('FAIL interrupted')
        with self.assertRaises(AssertionError): self.check()
    def test_stale_report_from_previous_device_run_is_rejected(self):
        self.report['utc'] = '2020-01-01T00:00:00.0000000Z'
        with self.assertRaises(AssertionError): self.check()
    def test_truncated_or_unordered_samples_are_rejected(self):
        for samples in [[{'elapsedSeconds':0.01}], [{'elapsedSeconds':x} for x in [0,10,5,15,20]]]:
            value=copy.deepcopy(self.report); value['samples']=samples
            with self.assertRaises(AssertionError): self.check(value)
    def test_wrong_model_debug_partial_or_inactive_run_is_rejected(self):
        for field,value in [('model','classic'),('completed',False),('developmentBuild',True),('measuredSeconds',19),('playingSeconds',2),('usesByActor',[1,1,0,1,1]),('error','runtime failure')]:
            report=copy.deepcopy(self.report); report[field]=value
            with self.assertRaises(AssertionError): self.check(report)

if __name__ == '__main__': unittest.main()
