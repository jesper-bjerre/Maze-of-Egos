"""Compatibility entrypoint for the shared character generator (defaults to Putin)."""
from pathlib import Path
exec(compile(Path(__file__).with_name('build_character.py').read_text(),str(Path(__file__).with_name('build_character.py')),'exec'))
