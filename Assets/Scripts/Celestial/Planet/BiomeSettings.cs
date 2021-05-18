using UnityEngine;

namespace SpaceGame.Celestial
{
    [CreateAssetMenu()]
    public class BiomeSettings : ScriptableObject
    {
        public Biome[] biomes;

        [System.Serializable]
        public class Biome 
        {
            public Gradient gradient;
            public Color tint;
            public float startHeight;
        }
    }
}