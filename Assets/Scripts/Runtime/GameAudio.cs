using MazeOfEgos.Core;
using UnityEngine;

namespace MazeOfEgos.Runtime
{
    public sealed class GameAudio
    {
        private readonly GameApp app;
        private readonly AudioSource effects, music;
        private readonly AudioSource[] pursuers = new AudioSource[4];
        private readonly AudioClip heart, power, slip, caught, encounter;
        private readonly AudioClip[] signatures = new AudioClip[4];
        private readonly double[] nextCue = new double[4];
        public GameAudio(GameApp app)
        {
            this.app = app;
            effects = new GameObject("Original cartoon effects", typeof(AudioSource)).GetComponent<AudioSource>();
            heart = Tone("Heart", 660, 880, 0.065f); power = Tone("Power heart", 440, 1000, 0.35f);
            slip = Tone("Comic slip", 800, 190, 0.25f); caught = Tone("Caught", 390, 210, 0.35f);
            encounter = Tone("Comic surprise", 480, 760, 0.16f);
            for (int i = 0; i < 4; i++)
            {
                signatures[i] = Tone("Fictional pursuer sound " + i, 170 + i * 55, 210 + i * 75, 0.15f + i * 0.025f);
                pursuers[i] = new GameObject("Spatial pursuer cue " + i, typeof(AudioSource)).GetComponent<AudioSource>();
                pursuers[i].spatialBlend = 1; pursuers[i].rolloffMode = AudioRolloffMode.Logarithmic;
                pursuers[i].minDistance = 1.2f; pursuers[i].maxDistance = 16; pursuers[i].volume = 0.3f;
            }
            music = new GameObject("Original playful melody", typeof(AudioSource)).GetComponent<AudioSource>();
            music.clip = Melody(); music.loop = true; music.volume = 0.045f; music.Play();
        }
        public void Update(GameSession session, bool quiet)
        {
            music.mute = !app.Settings.music;
            for (int i = 0; i < 4; i++)
            {
                AudioSource source = pursuers[i]; source.mute = !app.Settings.sound || quiet;
                source.transform.position = MazeView.World(session.Maze.Near(session.Pursuers[i].Position(session.Maze), session.Player), 1.4f);
                if (quiet || !app.Settings.sound || session.Pursuers[i].life != PursuerLife.Active || session.Time < nextCue[i]) continue;
                nextCue[i] = session.Time + 0.75 + i * 0.1;
                source.pitch = session.IsFrightened(session.Pursuers[i]) ? 1.4f : 1;
                source.PlayOneShot(signatures[i]);
            }
        }
        public void Event(GameEvent e)
        {
            if (!app.Settings.sound) return;
            AudioClip clip = e.Kind == EventKind.Heart ? heart : e.Kind == EventKind.PowerHeart ? power :
                e.Kind == EventKind.Caught ? caught : e.Kind == EventKind.Slip ? slip :
                e.Kind == EventKind.PursuerPaused || e.Kind == EventKind.LevelClear || e.Kind == EventKind.WallWarning || e.Kind == EventKind.ItemUsed ? power : null;
            if (clip) effects.PlayOneShot(clip, e.Kind == EventKind.Heart ? 0.13f : 0.3f);
        }
        public void Encounter(int index)
        { if (app.Settings.sound) effects.PlayOneShot(encounter, app.Settings.reducedMotion ? 0.12f : 0.3f); }
        private static AudioClip Tone(string name, float start, float end, float seconds)
        {
            const int rate = 22050; var data = new float[(int)(rate * seconds)]; float phase = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float fraction = (float)i / data.Length; phase += Mathf.Lerp(start, end, fraction) * 2 * Mathf.PI / rate;
                data[i] = (Mathf.Sin(phase) + 0.18f * Mathf.Sin(phase * 2)) * Mathf.Sin(fraction * Mathf.PI) * 0.55f;
            }
            AudioClip clip = AudioClip.Create(name, data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        private static AudioClip Melody()
        {
            const int rate = 22050; const float beat = 0.32f;
            int[] notes = { 60, 64, 67, 64, 62, 65, 69, 65, 59, 62, 67, 62, 60, 64, 67, 72 };
            var data = new float[(int)(notes.Length * beat * rate)];
            for (int i = 0; i < data.Length; i++)
            {
                float time = (float)i / rate; int note = Mathf.Min(notes.Length - 1, (int)(time / beat));
                float local = time % beat, frequency = 440 * Mathf.Pow(2, (notes[note] - 69) / 12f);
                data[i] = Mathf.Sin(2 * Mathf.PI * frequency * local) * Mathf.Exp(-local * 12) * Mathf.Min(local * 150, 1);
            }
            AudioClip clip = AudioClip.Create("Original garden melody", data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
    }
}
