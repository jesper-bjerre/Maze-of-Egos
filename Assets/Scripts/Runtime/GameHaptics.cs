using MazeOfEgos.Core;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
namespace MazeOfEgos.Runtime
{
    public static class GameHaptics
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void MazeHaptic(int kind);
#endif
        public static void Play(EventKind kind)
        {
            int style = kind == EventKind.Caught ? 2 : kind == EventKind.PowerHeart || kind == EventKind.PursuerPaused ? 1 :
                kind == EventKind.ItemPicked || kind == EventKind.ItemUsed || kind == EventKind.WallWarning ? 0 : -1;
            if (style < 0) return;
#if UNITY_IOS && !UNITY_EDITOR
            MazeHaptic(style);
#elif UNITY_ANDROID && !UNITY_EDITOR
            if (style > 0) UnityEngine.Handheld.Vibrate();
#endif
        }
    }
}
