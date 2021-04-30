using SpaceGame.Utils;
using UnityEngine;

namespace SpaceGame.Celestial
{
    public class ShapeGenerator
    {
        public ShapeSettings shapeSettings;
        public MinMax elevationMinMax;

        private INoiseFilter[] noiseFilters;

        public void UpdateSettings(ShapeSettings _shapeSettings)
        {
            shapeSettings = _shapeSettings;

            noiseFilters = new INoiseFilter[shapeSettings.noiseLayers.Length];
            for (int i = 0; i < noiseFilters.Length; i++)
            {
                noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(shapeSettings.noiseLayers[i].noiseSettings);
            }

            elevationMinMax = new MinMax();
        }

        public float CalculateUnscaledElevation(Vector3 pointOnUnitSphere)
        {
            float firstLayerValue = 0;
            float unscaledElevation = 0;

            if (noiseFilters.Length > 0)
            {
                firstLayerValue = noiseFilters[0].Evaluate(pointOnUnitSphere);
                if (shapeSettings.noiseLayers[0].enabled)
                {
                    unscaledElevation = firstLayerValue;
                }
            }

            for (int i = 1; i < noiseFilters.Length; i++)
            {
                if (shapeSettings.noiseLayers[i].enabled)
                {
                    float mask = (shapeSettings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                    unscaledElevation += noiseFilters[i].Evaluate(pointOnUnitSphere) * mask;
                }
            }

            elevationMinMax.AddValue(unscaledElevation);
            return unscaledElevation;
        }

        public float GetScaledElevation(float unscaledElevation)
        {
            float scaledElevation = Mathf.Max(0, unscaledElevation);
            scaledElevation = shapeSettings.radius * (1 + scaledElevation);
            elevationMinMax.AddValue(scaledElevation);

            return scaledElevation;
        }
    }
}