using System;
using UnityEngine;

namespace MazeOfEgos.Runtime
{
    [Serializable] public sealed class CharacterMaterial
    {
        public string name;
        public float[] color;
        public Color Color => new Color(color[0], color[1], color[2], color[3]);
    }
    [Serializable] public sealed class CharacterDefinition
    {
        public string id, displayName, referenceSource;
        public float strideMeters;
        public CharacterMaterial[] materials;
    }
    public static class CharacterRoster
    {
        [Serializable] private sealed class Collection { public CharacterDefinition[] characters = Array.Empty<CharacterDefinition>(); }
        private static CharacterDefinition[] definitions;
        public static CharacterDefinition[] All => definitions ?? (definitions = JsonUtility.FromJson<Collection>(Resources.Load<TextAsset>("Data/characters").text).characters);
        public static CharacterDefinition Get(string id) => Array.Find(All, c => c.id == id) ?? throw new InvalidOperationException("Missing character " + id);
    }
}
