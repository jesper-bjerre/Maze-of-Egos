using MazeOfEgos.Core;
using UnityEngine;

namespace MazeOfEgos.Runtime
{
    public sealed class GameAudio
    {
        private readonly GameApp app;
        private readonly AudioSource music;
        private readonly AudioSource[] effects = new AudioSource[8];
        private int voice;
        private bool powerActive, boostActive;
        private double lastTime;
        private readonly AudioClip pickup, banana, oil, boost, warning, opened, closed, clear, powerEnd, boostEnd;
        private readonly AudioSource[] pursuers = new AudioSource[4];
        private readonly AudioClip heart, power, slip, caught, encounter;
        private readonly AudioClip[] signatures = new AudioClip[4];
        private readonly double[] nextCue = new double[4];
        public GameAudio(GameApp app)
        {
            this.app = app;
            for (int i = 0; i < effects.Length; i++) effects[i] = new GameObject("Original effect voice " + i, typeof(AudioSource)).GetComponent<AudioSource>();
            pickup = Tone("Item sparkle", 1100, 1500, .13f); banana = Tone("Banana toss", 350, 820, .18f);
            oil = Tone("Oil plop", 220, 95, .24f); boost = Tone("Boost whoosh", 130, 680, .4f);
            warning = Tone("Brass warning", 520, 540, .20f); opened = Tone("Garden mechanism open", 180, 340, .4f);
            closed = Tone("Garden mechanism close", 340, 170, .4f); clear = Tone("Garden cleared", 660, 1320, .7f);
            powerEnd = Tone("Power fades", 990, 440, .3f); boostEnd = Tone("Boost fades", 680, 130, .25f);
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
            PlatformAudio.Configure();
        }
        public void Update(GameSession session, bool quiet)
        {
            music.mute = !app.Settings.music;
            bool powerNow = session.PowerUntil > session.Time, boostNow = session.BoostUntil > session.Time;
            if (session.Time >= lastTime && !quiet && app.Settings.sound)
            { if (powerActive && !powerNow) Play(powerEnd, .22f); if (boostActive && !boostNow) Play(boostEnd, .22f); }
            if (session.Time < lastTime) for (int i = 0; i < nextCue.Length; i++) nextCue[i] = 0;
            lastTime = session.Time; powerActive = powerNow; boostActive = boostNow;
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
                e.Kind == EventKind.PursuerPaused ? slip : e.Kind == EventKind.LevelClear ? clear : e.Kind == EventKind.WallWarning ? warning :
                e.Kind == EventKind.WallOpened ? opened : e.Kind == EventKind.WallClosed ? closed : e.Kind == EventKind.ItemPicked ? pickup :
                e.Kind == EventKind.ItemUsed ? ((ItemKind)e.Actor == ItemKind.SpeedBoost ? boost : (ItemKind)e.Actor == ItemKind.BananaPeel ? banana : oil) : null;
            if (clip) Play(clip, e.Kind == EventKind.Heart ? .13f : .3f);
        }
        public void Encounter(int index)
        { if (app.Settings.sound) Play(encounter, app.Settings.reducedMotion ? .12f : .3f); }
        private void Play(AudioClip clip, float volume)
        {
            // A fixed voice budget prevents a dense heart trail from accumulating voices.
            AudioSource source = effects[voice++ % effects.Length]; source.Stop(); source.clip = clip; source.volume = volume; source.Play();
        }
        internal static AudioClip Tone(string name, float start, float end, float seconds)
        {
            const int rate = 22050; var data = new float[(int)(rate * seconds)]; float phase = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float fraction = (float)i / data.Length; phase += Mathf.Lerp(start, end, fraction) * 2 * Mathf.PI / rate;
                data[i] = (Mathf.Sin(phase) + 0.18f * Mathf.Sin(phase * 2)) * Mathf.Sin(fraction * Mathf.PI) * 0.55f;
            }
            AudioClip clip = AudioClip.Create(name, data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        internal static AudioClip Melody()
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
