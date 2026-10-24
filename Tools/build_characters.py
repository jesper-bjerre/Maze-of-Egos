#!/usr/bin/env python3
"""Rebuild editable character sources and FBX exports with a local Blender installation."""
import argparse, hashlib, json, shutil, subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--blender')
    parser.add_argument('--character', choices=['all','putin','xi','kim','trump','zelenskyy'],default='all')
    args=parser.parse_args()
    blender=args.blender or shutil.which('blender') or str(ROOT/'.tools/blender/Blender.app/Contents/MacOS/Blender')
    entries=json.loads((ROOT/'Assets/Resources/Data/characters.json').read_text())['characters']
    (ROOT/'Logs').mkdir(exist_ok=True)
    for entry in entries:
        name=entry['id']
        if args.character not in ('all',name): continue
        log=ROOT/'Logs'/('blender-'+name+'.log')
        with log.open('w') as output:
            subprocess.run([blender,'--background','--python-exit-code','1','--python',str(ROOT/'Tools/build_character.py'),'--','--character',name],cwd=ROOT,stdout=output,stderr=subprocess.STDOUT,check=True)
        manifest=json.loads((ROOT/'art/characters'/name/'manifest.json').read_text())
        actual=hashlib.sha256((ROOT/manifest['runtimeModel']).read_bytes()).hexdigest()
        if manifest['id']!=name or manifest['runtimeSha256']!=actual or 'CHARACTER BUILT '+name not in log.read_text():
            raise RuntimeError('Incomplete character export: '+name)
        print('BUILT '+name,flush=True)
if __name__=='__main__': main()
