using UnityEngine;

namespace SpaceGame.Celestial
{
    [CreateAssetMenu()]
    public class ColourSettings : ScriptableObject
    {
        public Gradient terrainGradient;
        public Color deepOceanColour;
        public Color shallowOceanColour;
    }
}