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
            foreach (string name in new[] { "Idle", "Chase", "Surprise", "Slip", "Return" })
                if (animation[name] == null) throw new InvalidOperationException("Character missing animation " + name);
            skin = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            jacket = new bool[skin.sharedMaterials.Length];
            for (int i = 0; i < jacket.Length; i++) jacket[i] = skin.sharedMaterials[i].name == "Putin-SuitNavy" || skin.sharedMaterials[i].name == "Putin-SuitLapel";
            frightened.SetColor("_BaseColor", new Color(.40f, .57f, .90f));
        }
        public void Reset() { wasFrightened = false; surpriseStarted = double.NegativeInfinity; }
        public void Surprise(double time) => surpriseStarted = time;
        public void Render(GameSession session, PursuerState state, bool reducedMotion)
        {
            bool vulnerable = session.IsFrightened(state);
            if (vulnerable && !wasFrightened) Surprise(session.Time);
            wasFrightened = vulnerable;
            bool slip = session.Time < state.stunUntil || session.Time < state.oilUntil;
            string clip = state.life == PursuerLife.Returning ? "Return" : slip ? "Slip" :
                !reducedMotion && session.Time - surpriseStarted < .8 ? "Surprise" :
                session.PursuerSpeed(state) > 0 && (state.life == PursuerLife.Active || state.life == PursuerLife.Exiting) ? "Chase" : "Idle";
            // A held, legible slip pose replaces wobble when reduced motion is enabled.
            float time = clip == "Surprise" ? (float)(session.Time - surpriseStarted) :
                clip == "Slip" && reducedMotion ? .3f : (float)session.Time;
            Sample(clip, time);
            for (int i = 0; i < jacket.Length; i++)
                if (jacket[i]) skin.SetPropertyBlock(vulnerable ? frightened : null, i);
        }
        private void Sample(string name, float time)
        {
            foreach (AnimationState other in animation) { other.enabled = false; other.weight = 0; }
            AnimationState state = animation[name]; state.enabled = true; state.weight = 1; state.speed = 0;
            state.time = name == "Surprise" ? Mathf.Min(time, state.length) : Mathf.Repeat(time, state.length);
            animation.Sample(); state.enabled = false; CurrentClip = name;
        }
    }
}
