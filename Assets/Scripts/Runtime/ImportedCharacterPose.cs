using System;
using MazeOfEgos.Core;
using UnityEngine;

namespace MazeOfEgos.Runtime
{
    // Gameplay owns root movement and time. Sampling explicitly prevents Unity's
    // wall clock from animating a paused game, countdown, menu or restored save.
    public sealed class ImportedCharacterPose
    {
        private readonly Animation animation;
        private readonly SkinnedMeshRenderer skin;
        private readonly MaterialPropertyBlock frightened = new MaterialPropertyBlock();
        private readonly bool[] jacket;
        private static readonly string[] ClipNames = { "Idle", "Chase", "Surprise", "Slip", "Return", "Puff", "Recover", "Throw", "Cheer" };
        private readonly AnimationState[] clips = new AnimationState[9];
        private readonly GaitClock gait = new GaitClock();
        private bool wasFrightened;
        private double surpriseStarted = double.NegativeInfinity;
        private PursuerLife previousLife;
        private double puffStarted = double.NegativeInfinity;
        public GameObject Instance { get; }
        public string CurrentClip { get; private set; }
        public ImportedCharacterPose(Transform parent, GameObject model, string id = "putin")
        {
            GameObject instance = UnityEngine.Object.Instantiate(model, parent, false);
            Instance = instance;
            instance.name = id + " rigged character";
            Transform facing = Array.Find(instance.GetComponentsInChildren<Transform>(true), t => t.name == "FacingForward");
            if (!facing) throw new InvalidOperationException("Character model has no forward marker.");
            Vector3 forward = parent.InverseTransformVector(facing.position - instance.transform.position); forward.y = 0;
            instance.transform.localRotation = Quaternion.FromToRotation(forward.normalized, Vector3.forward);
            animation = instance.GetComponent<Animation>();
            if (!animation) animation = instance.AddComponent<Animation>();
            animation.playAutomatically = false; animation.Stop(); animation.cullingType = AnimationCullingType.AlwaysAnimate;
            foreach (AnimationClip clip in Resources.LoadAll<AnimationClip>("CharacterModels/" + (model.name.EndsWith("-hand", StringComparison.Ordinal) ? "zelenskyy-hand" : id)))
                if (!clip.name.StartsWith("__", StringComparison.Ordinal) && animation[clip.name] == null) animation.AddClip(clip, clip.name);
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i] = animation[ClipNames[i]];
                if (clips[i] == null && i >= 7 && id != "zelenskyy") continue;
                if (clips[i] == null) throw new InvalidOperationException("Character missing animation " + ClipNames[i]);
                clips[i].speed = 0;
            }
            skin = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            jacket = new bool[skin.sharedMaterials.Length];
            string prefix = CharacterRoster.Get(id).displayName;
            for (int i = 0; i < jacket.Length; i++) jacket[i] = skin.sharedMaterials[i].name == prefix + "-SuitNavy" || skin.sharedMaterials[i].name == prefix + "-SuitLapel";
            frightened.SetColor("_BaseColor", new Color(.40f, .57f, .90f));
        }
        public void Reset() { wasFrightened = false; surpriseStarted = double.NegativeInfinity; gait.Reset(); puffStarted = double.NegativeInfinity; previousLife = PursuerLife.Recovering; }
        public void Surprise(double time) => surpriseStarted = time;
        public void Render(GameSession session, PursuerState state, bool reducedMotion)
        {
            if (state.life == PursuerLife.Returning && previousLife != PursuerLife.Returning) puffStarted = session.Time;
            previousLife = state.life;
            bool vulnerable = session.IsFrightened(state);
            if (vulnerable && !wasFrightened) Surprise(session.Time);
            wasFrightened = vulnerable;
            bool slip = session.Time < state.stunUntil || session.Time < state.oilUntil;
            double speed = session.PursuerSpeed(state);
            bool walking = state.life == PursuerLife.Active || state.life == PursuerLife.Exiting;
            gait.Update(session.Time, walking ? speed * MazeMap.GridMeters : 0);
            int clip = state.life == PursuerLife.Returning ? (!reducedMotion && session.Time - puffStarted < .6 ? 5 : 4) :
                state.life == PursuerLife.Recovering && state.recoverUntil - session.Time <= 2 ? 6 : slip ? 3 :
                walking && !reducedMotion && session.Time - surpriseStarted < .8 ? 2 : walking && speed > 0 ? 1 : 0;
            // A held, legible slip pose replaces wobble when reduced motion is enabled.
            float time = clip == 5 ? (float)(session.Time - puffStarted) : clip == 6 ? (float)(2 - (state.recoverUntil - session.Time)) : clip == 2 ? (float)(session.Time - surpriseStarted) :
                clip == 1 ? (float)(gait.Cycles % 1) * clips[1].length :
                clip == 4 ? (float)(session.Time * 2.6 % 1) * clips[4].length :
                clip == 3 && reducedMotion ? .3f : (float)session.Time;
            Sample(clip, time);
            for (int i = 0; i < jacket.Length; i++)
                if (jacket[i]) skin.SetPropertyBlock(vulnerable ? frightened : null, i);
        }
        public void Preview(string clip, float time)
        { int index = Array.IndexOf(ClipNames, clip); if (index < 0 || clips[index] == null) throw new ArgumentException("Unknown clip " + clip); Sample(index, time); }
        private void Sample(int index, float time)
        {
            for (int i = 0; i < clips.Length; i++) { if (clips[i] != null) { clips[i].enabled = false; clips[i].weight = 0; } }
            AnimationState state = clips[index]; state.enabled = true; state.weight = 1;
            state.time = (index == 2 || index == 5 || index == 6 || index == 7) ? Mathf.Min(time, state.length) : Mathf.Repeat(time, state.length);
            animation.Sample(); state.enabled = false; CurrentClip = ClipNames[index];
        }
    }
}
