using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
namespace MazeOfEgos.Runtime
{
    public static class PlatformAudio
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int MazeAudioConfigurePlayback();
        [DllImport("__Internal")] private static extern int MazeAudioUsesPlayback();
#endif
        public static void Configure()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (MazeAudioConfigurePlayback() == 0) Debug.LogWarning("Playback audio is temporarily unavailable.");
#endif
        }
        public static bool PlaybackPolicyActive
        {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return MazeAudioUsesPlayback() != 0;
#else
                return true;
#endif
            }
        }
    }
}
