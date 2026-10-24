using System;
using System.IO;
using System.Xml;
using MazeOfEgos.Core;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MazeOfEgos.Editor
{
    public static class ProjectSetup
    {
        private const string Scene = "Assets/Scenes/Maze.unity";
        private const string Pipeline = "Assets/Settings/GardenPipeline.asset";
        [MenuItem("Tools/Maze of Egos/Configure project")]
        public static void Configure()
        {
            Directory.CreateDirectory("Assets/Settings"); AssetDatabase.Refresh();
            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Pipeline);
            if (!pipeline)
            {
                var renderer = ScriptableObject.CreateInstance<UniversalRendererData>(); renderer.name = "Garden renderer";
                AssetDatabase.CreateAsset(renderer, "Assets/Settings/GardenRenderer.asset");
                pipeline = UniversalRenderPipelineAsset.Create(renderer); pipeline.name = "Garden mobile pipeline";
                pipeline.msaaSampleCount = 2; pipeline.renderScale = 1; pipeline.shadowDistance = 25;
                pipeline.mainLightShadowmapResolution = 1024; pipeline.supportsHDR = false;
                AssetDatabase.CreateAsset(pipeline, Pipeline);
            }
            GraphicsSettings.defaultRenderPipeline = pipeline; QualitySettings.renderPipeline = pipeline;
            QualitySettings.vSyncCount = 0;
            PlayerSettings.productName = "Maze of Egos"; PlayerSettings.companyName = "Maze of Egos";
            PlayerSettings.bundleVersion = "0.2.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "dk.mazeofegos.game");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetOSVersionString = "17.0";
            PlayerSettings.iOS.buildNumber = "17";
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.runInBackground = false;
            Texture2D icon = Resources.Load<Texture2D>("Brand/AppIcon");
            if (!icon) throw new InvalidOperationException("Missing original app icon.");
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
            int[] sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.iOS, IconKind.Any);
            var icons = new Texture2D[sizes.Length]; for (int i = 0; i < icons.Length; i++) icons[i] = icon;
            PlayerSettings.SetIcons(NamedBuildTarget.iOS, icons, IconKind.Any);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Scene, true) };
            var settingsAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (settingsAssets.Length > 0)
            {
                var settings = new SerializedObject(settingsAssets[0]);
                SerializedProperty handler = settings.FindProperty("activeInputHandler");
                if (handler != null && handler.intValue != 1) { handler.intValue = 1; settings.ApplyModifiedPropertiesWithoutUndo(); }
            }
            foreach (string id in new[] { "zelenskyy", "putin", "xi", "kim", "trump" })
            {
                string path = "Assets/Resources/Characters/" + id + ".png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (!importer) continue;
                if (importer.textureType != TextureImporterType.Default || importer.textureShape != TextureImporterShape.Texture2D ||
                    importer.maxTextureSize != 1024 || importer.mipmapEnabled || importer.wrapMode != TextureWrapMode.Clamp ||
                    importer.textureCompression != TextureImporterCompression.Compressed)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.maxTextureSize = 1024; importer.mipmapEnabled = false;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    importer.SaveAndReimport();
                }
            }
            var menuArt = AssetImporter.GetAtPath("Assets/Resources/Brand/MenuBackground.png") as TextureImporter;
            if (menuArt && (menuArt.maxTextureSize != 2048 || menuArt.mipmapEnabled || menuArt.npotScale != TextureImporterNPOTScale.None ||
                menuArt.textureCompression != TextureImporterCompression.CompressedHQ || menuArt.wrapMode != TextureWrapMode.Clamp))
            {
                menuArt.textureType = TextureImporterType.Default; menuArt.textureShape = TextureImporterShape.Texture2D;
                menuArt.maxTextureSize = 2048; menuArt.mipmapEnabled = false; menuArt.npotScale = TextureImporterNPOTScale.None;
                menuArt.wrapMode = TextureWrapMode.Clamp; menuArt.textureCompression = TextureImporterCompression.CompressedHQ;
                menuArt.SaveAndReimport();
            }
            // Resource materials retain the actual lit/emissive/instancing variants.
            if (!Resources.Load<Material>("Materials/GardenLit") || !Resources.Load<Material>("Materials/GardenEmissive"))
                throw new InvalidOperationException("Missing runtime material resources.");
            CharacterModelSetup.Configure();
            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.OpenScene(Scene); EditorSceneManager.SaveScene(scene);
            Debug.Log("Maze of Egos configured: Unity 6000.3.26f1, URP, landscape, Input System, iOS 17.");
        }
        [MenuItem("Tools/Maze of Egos/Validate content")]
        public static void Validate()
        {
            var data = JsonConvert.DeserializeObject<MazeDefinition>(File.ReadAllText("Assets/Resources/Data/classic_reference.json"));
            var map = new MazeMap(data);
            if (map.Hearts.Count != 244) throw new InvalidOperationException("Wrong heart count.");
            var session = new GameSession(map); session.Pause();
            var roundTrip = JsonConvert.DeserializeObject<SessionSnapshot>(JsonConvert.SerializeObject(session.Snapshot()));
            session.Restore(roundTrip);
            foreach (string id in new[] { "zelenskyy", "putin", "xi", "kim", "trump" })
                if (!Resources.Load<Texture2D>("Characters/" + id)) throw new InvalidOperationException("Missing character reference " + id);
            if (!Resources.Load<Texture2D>("Brand/MenuBackground")) throw new InvalidOperationException("Missing menu cover art.");
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(Scene)) throw new InvalidOperationException("Missing playable scene.");
            CharacterModelSetup.Validate();
            var competitionRules=JsonConvert.DeserializeObject<CompetitionRules>(File.ReadAllText("Assets/Resources/Data/competition_rules.json")); competitionRules.Validate();
            var competition = new CompetitionSession(data, "trump", 123, competitionRules, countdown: false, computerPlayers: true);
            for (int tick = 0; tick < 180; tick++)
            {
                if (tick % 30 == 0) foreach (string id in CompetitionSession.ParticipantIds)
                    competition.ApplyCommand(new CompetitionCommand(id, competition.TickIndex, tick / 30 % 4));
                competition.Tick();
            }
            string competitionJson = JsonConvert.SerializeObject(competition.Snapshot());
            var restoredCompetition = new CompetitionSession(data);
            restoredCompetition.Restore(JsonConvert.DeserializeObject<CompetitionSnapshot>(competitionJson)); restoredCompetition.Resume(false);
            if (competitionJson != JsonConvert.SerializeObject(restoredCompetition.Snapshot()))
                throw new InvalidOperationException("Competition roundtrip failed in Unity.");
            Debug.Log("COMPETITION VALIDATION PASSED: five stable IDs, shared navigation, combat/AI rules, v4 snapshot and actual Unity JSON.");
            Debug.Log("CONTENT VALIDATION PASSED: 244 hearts, save roundtrip, five references, playable scene.");
        }
        public static void ConfigureAndValidate() { Configure(); Validate(); }
        [MenuItem("Tools/Maze of Egos/Build iOS Xcode project")]
        public static void BuildIOS() => Build(BuildTarget.iOS, "Builds/iOS");
        [MenuItem("Tools/Maze of Egos/Build macOS prototype")]
        public static void BuildMac() => Build(BuildTarget.StandaloneOSX, "Builds/macOS/Maze of Egos.app");
        [MenuItem("Tools/Maze of Egos/Build Windows prototype")]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Builds/Windows/Maze of Egos.exe");
        public static void BuildIOSPerformance() => Build(BuildTarget.iOS, "Builds/iOS-Performance", true);
        public static void BuildMacPerformance() => Build(BuildTarget.StandaloneOSX, "Builds/macOS-Performance/Maze of Egos.app", true);
        public static void BuildIOSRelease() => Build(BuildTarget.iOS, "Builds/iOS-Release", release: true);
        private static void Build(BuildTarget target, string output, bool performance = false, bool release = false)
        {
            Configure(); Validate(); Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { Scene }, locationPathName = output, target = target,
                options = performance || release ? BuildOptions.None : BuildOptions.Development,
                extraScriptingDefines = performance ? new[] { "MAZE_PERFORMANCE" } : Array.Empty<string>() });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Build failed: " + report.summary.result + ", errors: " + report.summary.totalErrors);
            if (target == BuildTarget.iOS)
            {
                // The product UI and shipped localization are English.
                string infoPath = Path.Combine(output, "Info.plist");
                var info = new XmlDocument { XmlResolver = null }; info.Load(infoPath);
                info.SelectSingleNode("/plist/dict/key[text()='CFBundleDevelopmentRegion']/following-sibling::*[1]").InnerText = "en";
                XmlNode dictionary = info.SelectSingleNode("/plist/dict");
                // Replacing this value also makes repeated exports safe.
                XmlNode oldKey = dictionary.SelectSingleNode("key[text()='CFBundleLocalizations']");
                if (oldKey != null)
                {
                    XmlNode oldValue = oldKey.NextSibling;
                    if (oldValue != null) dictionary.RemoveChild(oldValue);
                    dictionary.RemoveChild(oldKey);
                }
                XmlElement key = info.CreateElement("key"); key.InnerText = "CFBundleLocalizations"; dictionary.AppendChild(key);
                XmlElement list = info.CreateElement("array"), language = info.CreateElement("string"); language.InnerText = "en";
                list.AppendChild(language); dictionary.AppendChild(list);
                // Mono serializes an empty DTD subset as [] which Apple's plist parser rejects.
                // XML property lists do not require a DOCTYPE declaration.
                if (info.DocumentType != null) info.RemoveChild(info.DocumentType);
                info.Save(infoPath);
            }
            Debug.Log("BUILD SUCCEEDED: " + Path.GetFullPath(output));
        }
    }
}
