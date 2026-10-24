using System;
using System.Linq;
using UnityEditor;
using MazeOfEgos.Runtime;
using UnityEngine;

namespace MazeOfEgos.Editor
{
    // FBX is committed; Blender is needed only when editing/regenerating the pilot.
    public static class CharacterModelSetup
    {
        public static void Configure()
        {
            foreach (var character in CharacterRoster.All) ConfigureModel(character, character.id);
            ConfigureModel(CharacterRoster.Get("zelenskyy"), "zelenskyy-hand", true);
        }
        private static void ConfigureModel(CharacterDefinition definition, string id, bool hand = false)
        {
            string modelPath = "Assets/Resources/CharacterModels/" + id + ".fbx";
            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (!importer) throw new InvalidOperationException("Missing character FBX: " + id + ".");
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
            foreach (var entry in definition.materials)
            {
                string path = folder + "/" + definition.displayName + "-" + entry.name + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!material)
                {
                    material = new Material(Resources.Load<Material>("Materials/GardenLit"));
                    AssetDatabase.CreateAsset(material, path);
                }
                material.name = definition.displayName + "-" + entry.name; material.color = entry.Color;
                if (entry.name == "FaceReference")
                {
                    material.shader = Shader.Find("MazeOfEgos/Universal Render Pipeline/CharacterReference");
                    if (!material.shader) throw new InvalidOperationException("Missing character reference shader.");
                    material.SetColor("_SkinColor", Array.Find(definition.materials, m => m.name == "Skin").Color);
                }
                material.SetFloat("_Metallic", entry.name == "Metal" ? .65f : 0);
                material.SetFloat("_Smoothness", entry.name == "Leather" ? .66f : .28f);
                material.mainTexture = entry.name == "FaceReference" ? Resources.Load<Texture2D>("Characters/" + definition.id) : null;
                EditorUtility.SetDirty(material);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), entry.name), material);
            }
            importer.SaveAndReimport(); AssetDatabase.SaveAssets(); ValidateModel(definition, id, hand);
        }
        public static void Validate()
        {
            foreach (var character in CharacterRoster.All) ValidateModel(character, character.id);
            ValidateModel(CharacterRoster.Get("zelenskyy"), "zelenskyy-hand", true);
        }
        private static void ValidateModel(CharacterDefinition definition, string id, bool hand = false)
        {
            string modelPath = "Assets/Resources/CharacterModels/" + id + ".fbx";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (!model) throw new InvalidOperationException("Pilot FBX failed to import.");
            var skins = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (skins.Length != 1 || !skins[0].sharedMesh || skins[0].bones.Length < 17)
                throw new InvalidOperationException("Pilot needs one skinned mesh and its 17-bone rig.");
            var skin = skins[0];
            Color[] mask = skin.sharedMesh.colors;
            if (!hand && (mask.Length != skin.sharedMesh.vertexCount || !mask.Any(c => c.r > .05f && c.r < .95f)))
                throw new InvalidOperationException("Head reference fade weights were lost during FBX import.");
            Material face = skin.sharedMaterials.FirstOrDefault(m => m && m.name == definition.displayName + "-FaceReference");
            if (!hand && (!face || !face.mainTexture || face.shader.name != "MazeOfEgos/Universal Render Pipeline/CharacterReference"))
                throw new InvalidOperationException("Head must retain the original reference and skin-blend shader.");
            if (skin.sharedMesh.triangles.Length / 3 > 20000)
                throw new InvalidOperationException("Pilot exceeds its 20k triangle budget.");
            if (!hand && (skin.bounds.size.y < 1.7f || skin.bounds.size.y > 2.3f))
                throw new InvalidOperationException("Pilot imported at wrong scale: " + skin.bounds.size);
            foreach (var material in skin.sharedMaterials)
                if (!material || !material.shader || !material.shader.name.Contains("Universal Render Pipeline"))
                    throw new InvalidOperationException("Pilot material must use URP.");
            var animationClips = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>().ToArray();
            foreach (string name in new[] { "Idle", "Chase", "Surprise", "Slip", "Return", "Puff", "Recover" })
                if (!animationClips.Any(c => c.name == name && c.legacy && c.length > .1f))
                    throw new InvalidOperationException("Missing usable legacy pilot animation: " + name + "; imported: " + string.Join(", ", animationClips.Select(c => c.name)));
            if (definition.id == "zelenskyy")
                foreach (string name in new[] { "Throw", "Cheer" })
                    if (!animationClips.Any(c => c.name == name && c.legacy && c.length > .1f))
                        throw new InvalidOperationException("Hero missing " + name);
            Debug.Log("CHARACTER VALIDATION PASSED: " + id + ", " + skin.sharedMesh.triangles.Length / 3 + " triangles, " + skin.bones.Length + " bones, seven clips, metre scale, URP.");
        }
    }
}
