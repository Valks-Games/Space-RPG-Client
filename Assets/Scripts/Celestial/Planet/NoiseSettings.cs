using UnityEngine;

namespace SpaceGame.Celestial
{
    [System.Serializable]
    public class NoiseSettings
    {
        public enum FilterType { Simple, Ridgid };

        public FilterType filterType;

        [ConditionalHide("filterType", 0)]
        public SimpleNoiseSettings simpleNoiseSettings;

        [ConditionalHide("filterType", 1)]
        public RidgidNoiseSettings ridgidNoiseSettings;

        [System.Serializable]
        public class SimpleNoiseSettings
        {
            [Range(0, 1)]
            public float strength = 1;

            [Range(1, 8)]
            public int numLayers = 1;

            [Range(0, 2)]
            public float baseRoughness = 1;

            [Range(0, 10)]
            public float frequency = 2;

            [Range(0, 3)]
            public float amplitude = .5f;

            [Range(0, 1)]
            public float minValue;

            public Vector3 centre;
        }

        [System.Serializable]
        public class RidgidNoiseSettings : SimpleNoiseSettings
        {
            public float weightMultiplier = .8f;
        }
    }
}