using MazeOfEgos.Core;
using UnityEngine;

namespace MazeOfEgos.Runtime
{
    // Isolated preview stage; the game camera never renders this layer.
    public sealed class HeroPresentation
    {
        public readonly RenderTexture Texture;
        private readonly Camera camera;
        private readonly ImportedCharacterPose pose;
        private float poseTime;
        public HeroPresentation()
        {
            const int layer = 30;
            var stage = new GameObject("Hero portrait stage").transform;
            stage.position = new Vector3(1000, 0, 0);
            pose = new ImportedCharacterPose(stage, Resources.Load<GameObject>("CharacterModels/zelenskyy"), "zelenskyy");
            foreach (Transform child in stage.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = layer;
            pose.Instance.transform.localRotation *= Quaternion.Euler(0, -12, 0);
            Texture = new RenderTexture(384, 512, 16, RenderTextureFormat.ARGB32) { name = "Animated hero portrait", antiAliasing = 2 };
            Texture.Create();
            camera = new GameObject("Hero portrait camera", typeof(Camera)).GetComponent<Camera>();
            camera.transform.position = stage.position + new Vector3(0, 1.1f, 3.5f);
            camera.transform.LookAt(stage.position + Vector3.up);
            camera.orthographic = true; camera.orthographicSize = 1.12f;
            camera.nearClipPlane = .1f; camera.farClipPlane = 6;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.055f, .10f, .12f, 1);
            camera.cullingMask = 1 << layer; camera.targetTexture = Texture; camera.depth = -1;
        }
        public void Update(bool visible, bool celebrate, bool reducedMotion)
        {
            camera.enabled = visible;
            if (!visible) { poseTime = 0; return; }
            if (!reducedMotion) poseTime += Time.unscaledDeltaTime;
            pose.Preview(celebrate ? "Cheer" : "Idle", poseTime);
        }
    }
}
