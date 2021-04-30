using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame.Celestial 
{
    public class ColourGenerator
    {
        public ColourSettings colourSettings;
        private ShapeGenerator shapeGenerator;
        private Texture2D texture;
        private const int textureResolution = 50;

        public ColourGenerator(ColourSettings _colourSettings, ShapeGenerator _shapeGenerator)
        {
            colourSettings = _colourSettings;
            shapeGenerator = _shapeGenerator;
        }

        public void GenerateColours(PlanetMeshChunk _planetMeshChunk)
        {
            GenerateTerrainColours(_planetMeshChunk);
            GenerateOceanColours(_planetMeshChunk);
        }

        public void GenerateTerrainColours(PlanetMeshChunk _planetMeshChunk)
        {
            // Terrain shader uses both colour gradient and texture for nice effect
            // Create texture for terrain colour gradient
            if (texture == null)
                texture = new Texture2D(textureResolution, 1, TextureFormat.RGBA32, false);

            // Initialize colours for texture
            Color[] colours = new Color[textureResolution];

            // Calculate colours for texture
            for (int i = 0; i < textureResolution; i++)
                colours[i] = colourSettings.terrainGradient.Evaluate(i / (textureResolution - 1f));

            texture.SetPixels(colours);
            texture.Apply();

            // Apply texture to chunk
            _planetMeshChunk.meshRenderer.sharedMaterial.SetVector("_elevationMinMax", new Vector4(shapeGenerator.elevationMinMax.Min, shapeGenerator.elevationMinMax.Max));
            _planetMeshChunk.meshRenderer.sharedMaterial.SetTexture("_texture", texture);
        }

        public void GenerateOceanColours(PlanetMeshChunk _planetMeshChunk)
        {
            // If no ocean is set to false then do not populate ocean colours
            if (!shapeGenerator.shapeSettings.ocean)
                return;

            _planetMeshChunk.meshRenderer.sharedMaterial.SetColor("_deepOceanColor", colourSettings.deepOceanColour);
            _planetMeshChunk.meshRenderer.sharedMaterial.SetColor("_shallowOceanColor", colourSettings.shallowOceanColour);
        }
    }
}

