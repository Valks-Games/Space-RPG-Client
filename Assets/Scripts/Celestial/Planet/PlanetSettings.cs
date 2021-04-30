using UnityEngine;

namespace SpaceGame.Celestial
{
    [CreateAssetMenu()]
    public class PlanetSettings : ScriptableObject
    {
        public string planetName;

        [TextArea]
        public string description;

        public float treeDensity = 0.2f;
    }
}