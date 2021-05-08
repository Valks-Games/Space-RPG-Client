using SpaceGame.Utils;
using UnityEngine;

namespace SpaceGame.Celestial
{
    [ExecuteInEditMode]
    public class Planet : MonoBehaviour
    {
        public PlanetSettings planetSettings;
        public ShapeSettings shapeSettings;
        public ColourSettings colourSettings;

        private ShapeGenerator shapeGenerator = new ShapeGenerator();
        private ColourGenerator colourGenerator;

        private PlanetMeshChunkRenderer terrain;
        private PlanetMeshChunkRenderer ocean;

        private Transform parentTerrainChunks;
        private Transform parentOceanChunks;

        public MinMax elevationMinMax = new MinMax();

        [HideInInspector]
        public Vector3 curLocalPos;

        private const int textureResolution = 50;

        [HideInInspector]
        public bool planetSettingsFoldout;

        [HideInInspector]
        public bool shapeSettingsFoldout;

        [HideInInspector]
        public bool colourSettingsFoldout;

        public bool autoUpdate = true;

        // Planet meshes are generated in play mode.
        private void Awake()
        {
            GeneratePlanet();
        }

        private void Update()
        {
            // Only render chunks in range
        }

        public void GeneratePlanet()
        {
            if (SettingsIsInitialized()) 
            {
                SetParents();
                curLocalPos = transform.localPosition;

                shapeGenerator.UpdateSettings(shapeSettings);
                colourGenerator = new ColourGenerator(colourSettings, shapeGenerator);

                GeneratePlanetSettings();
                GenerateTerrainMesh();
                GenerateOceanMesh();
            }
        }

        private void SetParents()
        {
            if (parentTerrainChunks == null)
                parentTerrainChunks = transform.Find("Terrain Chunks");
            if (parentOceanChunks == null)
                parentOceanChunks = transform.Find("Ocean Chunks");
        }

        public void OnPlanetSettingsUpdated(bool buttonPressed)
        {
            if (autoUpdate || buttonPressed)
            {
                if (SettingsIsInitialized()) 
                {
                    GeneratePlanetSettings();
                }
            }
        }

        public void OnShapeSettingsUpdated(bool buttonPressed)
        {
            if (autoUpdate || buttonPressed)
            {
                if (SettingsIsInitialized())
                {
                    shapeGenerator.UpdateSettings(shapeSettings);
                    GenerateTerrainMesh();
                    GenerateOceanMesh();
                }
            }
        }

        public void OnColourSettingsUpdated(bool buttonPressed)
        {
            if (autoUpdate || buttonPressed)
            {
                if (SettingsIsInitialized())
                {
                    for (int i = 0; i < terrain.chunkData.Length; i++) 
                    {
                        if (terrain.chunkData[i].chunk != null)
                            terrain.colourGenerator.GenerateTerrainColours(terrain.chunkData[i].chunk);
                    }

                    for (int i = 0; i < ocean.chunkData.Length; i++)
                    {
                        if (ocean.chunkData[i].chunk != null)
                            ocean.colourGenerator.GenerateTerrainColours(ocean.chunkData[i].chunk);
                    }
                }
            }
        }

        private void GeneratePlanetSettings()
        {
            gameObject.name = $"Planet - {planetSettings.planetName}";
        }

        private void GenerateTerrainMesh()
        {
            // Remove old chunks
            if (parentTerrainChunks)
                DestroyImmediate(parentTerrainChunks.gameObject);

            if (!parentTerrainChunks)
            {
                parentTerrainChunks = new GameObject("Terrain Chunks").transform;
                parentTerrainChunks.parent = transform;
            }

            if (shapeGenerator == null)
                Debug.LogError("no shape generator");
            terrain = new PlanetMeshChunkRenderer(this, parentTerrainChunks, shapeGenerator, colourGenerator, PlanetMeshChunkRenderer.ShapeType.Noise);
        }

        private void GenerateOceanMesh()
        {
            // Remove old chunks
            if (parentOceanChunks)
                DestroyImmediate(parentOceanChunks.gameObject);

            if (!shapeSettings.ocean)
                return;

            if (!parentOceanChunks)
            {
                parentOceanChunks = new GameObject("Ocean Chunks").transform;
                parentOceanChunks.parent = transform;
            }

            if (shapeGenerator == null)
                Debug.LogError("no shape generator");
            ocean = new PlanetMeshChunkRenderer(this, parentOceanChunks, shapeGenerator, colourGenerator, PlanetMeshChunkRenderer.ShapeType.Sphere);
        }

        // Destroy procedurally generated meshes for file size reduction. Called in `DestroyOnSave.cs`
        public void Destroy()
        {
            if (parentTerrainChunks)
                while (parentTerrainChunks.childCount > 0)
                    DestroyImmediate(parentTerrainChunks.GetChild(0).gameObject);
            if (parentOceanChunks)
                while (parentOceanChunks.childCount > 0)
                    DestroyImmediate(parentOceanChunks.GetChild(0).gameObject);
        }

        private bool SettingsIsInitialized()
        {
            if (!planetSettings)
            {
                Debug.LogWarning("Planet settings has not been setup.");
                return false;
            }

            if (!shapeSettings)
            {
                Debug.LogWarning("Shape settings has not been setup.");
                return false;
            }

            if (!colourSettings)
            {
                Debug.LogWarning("Colour settings has not been setup.");
                return false;
            }

            return true;
        }
    }
}