using MazeOfEgos.Core;
using UnityEngine;

namespace MazeOfEgos.Runtime
{
    public sealed class CompetitionAudio
    {
        private readonly GameSettings settings;
        private readonly AudioSource music;
        private readonly AudioSource[] voices = new AudioSource[8];
        private readonly AudioClip[] clips = new AudioClip[8];
        private int voice;
        public CompetitionAudio(GameSettings settings)
        {
            this.settings = settings;
            for (int i = 0; i < voices.Length; i++) voices[i] = new GameObject("Competition effect " + i, typeof(AudioSource)).GetComponent<AudioSource>();
            for (int i = 0; i < clips.Length; i++) clips[i] = GameAudio.Tone("Playful effect " + i, 440 + i * 80, i == 4 ? 180 : 880 + i * 60, i == 0 ? .065f : .2f + i * .04f);
            music = new GameObject("Competition music", typeof(AudioSource)).GetComponent<AudioSource>();
            music.clip = GameAudio.Melody(); music.loop = true; music.volume = .045f; music.Play(); PlatformAudio.Configure();
        }
        public void Update() { music.mute = !settings.music; foreach (var source in voices) source.mute = !settings.sound; }
        public void Event(CompetitionEventKind kind, bool own)
        {
            if (!settings.sound || (kind == CompetitionEventKind.Heart && !own)) return;
            var source = voices[voice++ % voices.Length]; source.Stop(); source.clip = clips[(int)kind % clips.Length]; source.volume = kind == CompetitionEventKind.Heart ? .13f : own ? .26f : .12f; source.Play();
        }
    }
}
