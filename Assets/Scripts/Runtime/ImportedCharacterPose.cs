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
        private static readonly string[] ClipNames = { "Idle", "Chase", "Surprise", "Slip", "Return" };
        private readonly AnimationState[] clips = new AnimationState[5];
        private readonly GaitClock gait = new GaitClock();
        private bool wasFrightened;
        private double surpriseStarted = double.NegativeInfinity;
        public string CurrentClip { get; private set; }
        public ImportedCharacterPose(Transform parent, GameObject model)
        {
            GameObject instance = UnityEngine.Object.Instantiate(model, parent, false);
            instance.name = "Putin rigged review pilot";
            Transform facing = Array.Find(instance.GetComponentsInChildren<Transform>(true), t => t.name == "FacingForward");
            if (!facing) throw new InvalidOperationException("Character model has no forward marker.");
            Vector3 forward = parent.InverseTransformVector(facing.position - instance.transform.position); forward.y = 0;
            instance.transform.localRotation = Quaternion.FromToRotation(forward.normalized, Vector3.forward);
            animation = instance.GetComponent<Animation>();
            if (!animation) animation = instance.AddComponent<Animation>();
            animation.playAutomatically = false; animation.Stop(); animation.cullingType = AnimationCullingType.AlwaysAnimate;
            foreach (AnimationClip clip in Resources.LoadAll<AnimationClip>("CharacterModels/putin"))
                if (!clip.name.StartsWith("__", StringComparison.Ordinal) && animation[clip.name] == null) animation.AddClip(clip, clip.name);
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i] = animation[ClipNames[i]];
                if (clips[i] == null) throw new InvalidOperationException("Character missing animation " + ClipNames[i]);
                clips[i].speed = 0;
            }
            skin = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            jacket = new bool[skin.sharedMaterials.Length];
            for (int i = 0; i < jacket.Length; i++) jacket[i] = skin.sharedMaterials[i].name == "Putin-SuitNavy" || skin.sharedMaterials[i].name == "Putin-SuitLapel";
            frightened.SetColor("_BaseColor", new Color(.40f, .57f, .90f));
        }
        public void Reset() { wasFrightened = false; surpriseStarted = double.NegativeInfinity; gait.Reset(); }
        public void Surprise(double time) => surpriseStarted = time;
        public void Render(GameSession session, PursuerState state, bool reducedMotion)
        {
            bool vulnerable = session.IsFrightened(state);
            if (vulnerable && !wasFrightened) Surprise(session.Time);
            wasFrightened = vulnerable;
            bool slip = session.Time < state.stunUntil || session.Time < state.oilUntil;
            double speed = session.PursuerSpeed(state);
            bool walking = state.life == PursuerLife.Active || state.life == PursuerLife.Exiting;
            gait.Update(session.Time, walking ? speed * MazeMap.GridMeters : 0);
            int clip = state.life == PursuerLife.Returning ? 4 : slip ? 3 :
                walking && !reducedMotion && session.Time - surpriseStarted < .8 ? 2 : walking && speed > 0 ? 1 : 0;
            // A held, legible slip pose replaces wobble when reduced motion is enabled.
            float time = clip == 2 ? (float)(session.Time - surpriseStarted) :
                clip == 1 ? (float)(gait.Cycles % 1) * clips[1].length :
                clip == 4 ? (float)(session.Time * 2.6 % 1) * clips[4].length :
                clip == 3 && reducedMotion ? .3f : (float)session.Time;
            Sample(clip, time);
            for (int i = 0; i < jacket.Length; i++)
                if (jacket[i]) skin.SetPropertyBlock(vulnerable ? frightened : null, i);
        }
        private void Sample(int index, float time)
        {
            for (int i = 0; i < clips.Length; i++) { clips[i].enabled = false; clips[i].weight = 0; }
            AnimationState state = clips[index]; state.enabled = true; state.weight = 1;
            state.time = index == 2 ? Mathf.Min(time, state.length) : Mathf.Repeat(time, state.length);
            animation.Sample(); state.enabled = false; CurrentClip = ClipNames[index];
        }
    }
}
