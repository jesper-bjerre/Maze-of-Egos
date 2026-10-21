using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MazeOfEgos.Editor
{
    // FBX is committed; Blender is needed only when editing/regenerating the pilot.
    public static class CharacterModelSetup
    {
        private const string ModelPath = "Assets/Resources/CharacterModels/putin.fbx";
        private static readonly string[] Names = { "SuitNavy", "SuitLapel", "ShirtIvory", "TieBurgundy", "Leather", "Skin", "Hair", "Metal", "FaceReference" };
        private static readonly Color[] Colors = {
            new Color(.075f,.10f,.15f), new Color(.105f,.135f,.19f), new Color(.91f,.90f,.84f),
            new Color(.38f,.035f,.055f), new Color(.035f,.028f,.025f), new Color(.72f,.46f,.31f),
            new Color(.28f,.18f,.10f), new Color(.52f,.35f,.13f), Color.white };
        public static void Configure()
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (!importer) throw new InvalidOperationException("Missing Putin pilot FBX.");
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true; importer.importBlendShapes = false;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            importer.globalScale = 1; importer.useFileScale = true;
            importer.importNormals = ModelImporterNormals.Import;
            importer.isReadable = false;
            // First import discovers FBX take names before their stable runtime names are assigned.
            importer.SaveAndReimport();
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                clip.name = clip.takeName.Substring(clip.takeName.LastIndexOf('|') + 1);
                clip.loopTime = clip.name == "Idle" || clip.name == "Chase" || clip.name == "Return";
                clip.wrapMode = clip.loopTime ? WrapMode.Loop : WrapMode.ClampForever;
            }
            importer.clipAnimations = clips;
            string folder = "Assets/Resources/CharacterModels/Materials";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Resources/CharacterModels", "Materials");
            for (int i = 0; i < Names.Length; i++)
            {
                string path = folder + "/Putin-" + Names[i] + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!material)
                {
                    material = new Material(Resources.Load<Material>("Materials/GardenLit"));
                    AssetDatabase.CreateAsset(material, path);
                }
                material.name = "Putin-" + Names[i]; material.color = Colors[i];
                material.SetFloat("_Metallic", Names[i] == "Metal" ? .65f : 0);
                material.SetFloat("_Smoothness", Names[i] == "Leather" ? .66f : .28f);
                material.mainTexture = Names[i] == "FaceReference" ? Resources.Load<Texture2D>("Characters/putin") : null;
                EditorUtility.SetDirty(material);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), Names[i]), material);
            }
            importer.SaveAndReimport(); AssetDatabase.SaveAssets(); Validate();
        }
        public static void Validate()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (!model) throw new InvalidOperationException("Pilot FBX failed to import.");
            var skins = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (skins.Length != 1 || !skins[0].sharedMesh || skins[0].bones.Length < 17)
                throw new InvalidOperationException("Pilot needs one skinned mesh and its 17-bone rig.");
            var skin = skins[0];
            if (skin.sharedMesh.triangles.Length / 3 > 20000)
                throw new InvalidOperationException("Pilot exceeds its 20k triangle budget.");
            if (skin.bounds.size.y < 1.7f || skin.bounds.size.y > 2.3f)
                throw new InvalidOperationException("Pilot imported at wrong scale: " + skin.bounds.size);
            foreach (var material in skin.sharedMaterials)
                if (!material || !material.shader || !material.shader.name.Contains("Universal Render Pipeline"))
                    throw new InvalidOperationException("Pilot material must use URP.");
            var animationClips = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().ToArray();
            foreach (string name in new[] { "Idle", "Chase", "Surprise", "Slip", "Return" })
                if (!animationClips.Any(c => c.name == name && c.legacy && c.length > .1f))
                    throw new InvalidOperationException("Missing usable legacy pilot animation: " + name + "; imported: " + string.Join(", ", animationClips.Select(c => c.name)));
            Debug.Log("CHARACTER VALIDATION PASSED: Putin, " + skin.sharedMesh.triangles.Length / 3 + " triangles, " + skin.bones.Length + " bones, five clips, metre scale, URP.");
        }
    }
}
