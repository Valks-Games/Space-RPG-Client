using SpaceGame.Utils;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SpaceGame.Celestial 
{
    public class EdgeRenderer : MonoBehaviour
    {
        // Terrain Settings
        public EdgeTerrainSettings terrainSettings;
        [HideInInspector] public bool terrainSettingsFoldout;

        // Ocean Settings
        public EdgeOceanSettings oceanSettings;
        [HideInInspector] public bool oceanSettingsFoldout;

        public Transform treeModel;
        public Transform grassModel;
        public EdgeChunkRaw[] chunks;
        public NoiseSettings treeNoiseSettings;
        public INoiseFilter[] terrainNoise;

        // Test Values
        public float blendRange3;
        public float blendRange4;

        private int chunksIndex;
        private int chunkNameIndex;
        private INoiseFilter biome;
        private INoiseFilter biomeBlendLine;

        private void ResetEverything()
        {
            // Destroy old meshes
            if (Application.isPlaying)
            {
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                var tempList = transform.Cast<Transform>().ToList();
                foreach (Transform child in tempList)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

        }

        public void GenerateTerrain()
        {
            ResetEverything();

            var subdividedIcosahedron = new IcosahedronSubdivided(terrainSettings.chunkSubdivisions);

            chunksIndex = 0;
            chunkNameIndex = 0;

            chunks = new EdgeChunkRaw[subdividedIcosahedron.chunkData.Length];

            // Populate chunks array
            var toggle = true;
            for (int i = 0; i < subdividedIcosahedron.chunkData.Length; i++)
            {
                if (toggle) // For expermenting with edge stitching
                    PrepareChunk(subdividedIcosahedron.chunkData[i], terrainSettings.chunkTriangleSubdivisions);
                else
                    PrepareChunk(subdividedIcosahedron.chunkData[i], terrainSettings.chunkTriangleSubdivisions);

                toggle = !toggle;
            }

            // Biomes and Noise
            /////////////////////////////////////////////
            // Prepare noise
            terrainNoise = new INoiseFilter[terrainSettings.biomes.Length];
            for (int i = 0; i < terrainSettings.biomes.Length; i++)
            {
                terrainNoise[i] = NoiseFilterFactory.CreateNoiseFilter(terrainSettings.biomes[i].terrainNoise);
            }

            biome = NoiseFilterFactory.CreateNoiseFilter(terrainSettings.biomeNoise);
            biomeBlendLine = NoiseFilterFactory.CreateNoiseFilter(terrainSettings.biomeNoiseLine);

            if (terrainSettings.biomes.Length == 0)
            {
                Debug.LogWarning("At least one biome must be defined.");
                return;
            }

            if (terrainSettings.biomeStyle == BiomeStyle.Continent && terrainSettings.biomes.Length < 3)
            {
                Debug.LogWarning("Continent biome style requires exactly 3 biomes.");
                return;
            }
            
            for (int i = 0; i < chunks.Length; i++) 
            {
                var vertices = chunks[i].vertices;
                var colors = chunks[i].colors;

                if (terrainSettings.biomeStyle == BiomeStyle.Frequency) 
                {
                    BiomesSameFrequency(ref chunks[i].vertices, ref chunks[i].colors, terrainSettings.biomes.Length);
                }

                if (terrainSettings.biomeStyle == BiomeStyle.XY)
                {
                    XYBiomes(ref chunks[i].vertices, ref chunks[i].colors, terrainSettings.biomes.Length, true);
                }

                if (terrainSettings.biomeStyle == BiomeStyle.Continent) 
                {
                    BiomesContinentStyleFade(ref chunks[i].vertices, ref chunks[i].colors);
                }

                chunks[i].vertices = vertices;
                chunks[i].colors = colors;
            }

            /////////////////////////////////////////////

            
            IcosahedronSubdivided.StitchEdges(chunks);

            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i].GenerateMesh();
            }

            IcosahedronSubdivided.FixNormalEdges(chunks);

            var treeNoise = NoiseFilterFactory.CreateNoiseFilter(treeNoiseSettings);

            // Spawn Trees
            for (int i = 0; i < chunks.Length; i++)
            {
                for (int j = 0; j < chunks[i].vertices.Length; j++)
                {
                    var noiseValue = treeNoise.Evaluate(chunks[i].vertices[j]);

                    // steepness returns value between 0 and 90
                    var steepness = Vector3.Angle((chunks[i].vertices[j] - new Vector3(0, 0, 0)).normalized, chunks[i].mesh.normals[j]);

                    if (steepness < 45 && chunks[i].mesh.colors[j].g == 1) // Check steepness and biome
                    {

                        if ((chunks[i].vertices[j] - new Vector3(0, 0, 0)).magnitude > terrainSettings.radius + 0.25f) // Don't spawn in the water
                        {
                            if (noiseValue > 0.9f)
                            {
                                SpawnTree(i, j);
                            }
                            else
                            {
                                if (Random.value < 0.03f)
                                {
                                    SpawnTree(i, j);
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < chunks.Length; i++)
            {
                //chunks[i].PerfectSphereNormals();
            }

            GenerateOcean();
        }

        public void GenerateOcean() 
        {
            ChunkData[] oceanChunkData;

            // Ocean
            var ocean = new IcosahedronSubdivided(0);
            oceanChunkData = ocean.chunkData;

            for (int i = 0; i < oceanChunkData.Length; i++) 
            {
                var chunkObj = new GameObject();
                chunkObj.name = $"{chunkNameIndex++}";
                chunkObj.transform.SetParent(transform);
                chunkObj.AddComponent<MeshFilter>();
                var meshRenderer = chunkObj.AddComponent<MeshRenderer>();
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                var chunk = new EdgeChunkRaw(chunkObj, oceanChunkData[i].vertices, terrainSettings.chunkTriangleSubdivisions, oceanSettings.material);

                for (int j = 0; j < chunk.vertices.Length; j++) 
                {
                    chunk.vertices[j] = chunk.vertices[j].normalized * (terrainSettings.radius + oceanSettings.height);
                }

                chunk.GenerateMesh();
                oceanChunkData[i].chunk = chunk;
            }
        }

        /*
         * Same frequency.
         */
        // terrainNoise[0] RED
        // terrainNoise[1] GREEN
        // terrainNoise[2] BLUE
        // terrainNoise[3] BLACK
        private void BiomesSameFrequency(ref Vector3[] vertices, ref Color[] colors, int numBiomes)
        {
            var biomeColors = new Color[] { Color.green, Color.blue };

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = vertices[i].normalized;

                var biomeNoise = biome.Evaluate(vertices[i]);
                var blendLineNoise = biomeBlendLine.Evaluate(vertices[i]);
                var totalNoise = 0f;

                if (numBiomes >= 1)
                {
                    colors[i] = Color.red;
                    totalNoise = terrainNoise[0].Evaluate(vertices[i]);
                }

                if (numBiomes >= 2)
                {
                    if (biomeNoise < 0.6f + blendLineNoise)
                    {
                        var t = Utils.Utils.Remap(biomeNoise, 0.6f + blendLineNoise, 0.5f + blendLineNoise, 0, 1);
                        colors[i] = Color.Lerp(Color.red, Color.green, t);
                        totalNoise = Mathf.Lerp(terrainNoise[0].Evaluate(vertices[i]), terrainNoise[1].Evaluate(vertices[i]), t);
                    }

                    if (biomeNoise < 0.5f + blendLineNoise)
                    {
                        colors[i] = Color.green;
                        totalNoise = terrainNoise[1].Evaluate(vertices[i]);
                    }
                }

                if (numBiomes >= 3)
                {
                    if (biomeNoise < 0.4f + blendLineNoise)
                    {
                        var t = Utils.Utils.Remap(biomeNoise, 0.4f + blendLineNoise, 0.3f + blendLineNoise, 0, 1);
                        colors[i] = Color.Lerp(Color.green, Color.blue, t);
                        totalNoise = Mathf.Lerp(terrainNoise[1].Evaluate(vertices[i]), terrainNoise[2].Evaluate(vertices[i]), t);
                    }

                    if (biomeNoise < 0.3f + blendLineNoise)
                    {
                        colors[i] = Color.blue;
                        totalNoise = terrainNoise[2].Evaluate(vertices[i]);
                    }
                }

                if (numBiomes >= 4)
                {
                    if (biomeNoise < 0.2f + blendLineNoise)
                    {
                        var t = Utils.Utils.Remap(biomeNoise, 0.2f + blendLineNoise, 0.1f + blendLineNoise, 0, 1);
                        colors[i] = Color.Lerp(Color.blue, Color.black, t);
                        totalNoise = Mathf.Lerp(terrainNoise[2].Evaluate(vertices[i]), terrainNoise[3].Evaluate(vertices[i]), t);
                    }

                    if (biomeNoise < 0.1f + blendLineNoise)
                    {
                        colors[i] = Color.black;
                        totalNoise = terrainNoise[3].Evaluate(vertices[i]);
                    }
                }

                vertices[i] = vertices[i].normalized * (terrainSettings.radius + totalNoise);
            }
        }

        /*
         * XY Biomes.
         */
        private void XYBiomes(ref Vector3[] vertices, ref Color[] colors, int numBiomes, bool vert)
        {
            var biomeColors = new Color[] { Color.red, Color.green, Color.blue, Color.black };

            for (int i = 0; i < vertices.Length; i++)
            {
                float totalNoise;

                var axis = vert ? vertices[i].y : vertices[i].x;

                var height = Utils.Utils.Remap(axis, -1, 1, 0, 0.99f);
                var biomeIndex = Mathf.FloorToInt(height * (numBiomes));
                biomeIndex = Mathf.Clamp(biomeIndex, 0, numBiomes);

                colors[i] = biomeColors[biomeIndex]; // Biome Colors
                totalNoise = terrainNoise[biomeIndex].Evaluate(vertices[i]);

                var biome = height * numBiomes;

                // Blending Zones
                for (int n = 1; n < numBiomes; n++)
                {
                    if (biome > n - blendRange3 && biome < n + blendRange3)
                    {
                        var biomeLine = -1 + ((float)n / numBiomes) * 2;
                        var t = Utils.Utils.Remap(axis, biomeLine - blendRange3 / numBiomes, biomeLine + blendRange3 / numBiomes, 0, 1);
                        colors[i] = Color.Lerp(biomeColors[n - 1], biomeColors[n], t);
                        totalNoise = Mathf.Lerp(terrainNoise[n - 1].Evaluate(vertices[i]), terrainNoise[n].Evaluate(vertices[i]), t);
                    }
                }

                vertices[i] = vertices[i].normalized * (terrainSettings.radius + totalNoise);
            }
        }

        /*
         * All red.
         */
        private void OneBiome(ref Vector3[] vertices, ref Color[] colors)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                colors[i] = Color.red;
                var totalNoise = terrainNoise[0].Evaluate(vertices[i]);
                vertices[i] = vertices[i].normalized * (terrainSettings.radius + totalNoise);
            }
        }

        /*
         * 50% red near top, 50% green near bottom, surrounded by blue.
         */
        private void BiomesContinentStyleFade(ref Vector3[] vertices, ref Color[] colors)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                float totalNoise;

                var biomeNoise = biome.Evaluate(vertices[i]);
                var blendLine = biomeBlendLine.Evaluate(vertices[i]);

                if (biomeNoise > 0.5f)
                {
                    var percentBlend = Mathf.InverseLerp(0.5f, 1.0f, biomeNoise);

                    // 3 Biomes
                    // Blend blue with redGreen
                    var percentBlendLine = Mathf.InverseLerp(blendRange3, blendRange4, vertices[i].y + blendLine);
                    var r = 1 - percentBlendLine;
                    var g = percentBlendLine;

                    var redGreen = new Color(r, g, 0, 0);
                    var redGreenNoise = Mathf.Lerp(terrainNoise[0].Evaluate(vertices[i]), terrainNoise[1].Evaluate(vertices[i]), percentBlendLine);

                    totalNoise = Mathf.Lerp(terrainNoise[2].Evaluate(vertices[i]), redGreenNoise, percentBlend);
                    colors[i] = Color.Lerp(Color.blue, redGreen, percentBlend);
                }
                else
                {
                    totalNoise = terrainNoise[2].Evaluate(vertices[i]);
                    colors[i] = Color.blue;
                }

                vertices[i] = vertices[i].normalized * (terrainSettings.radius + totalNoise);
            }
        }

        private void SpawnTree(int chunkIndex, int vertexIndex)
        {
            var tree = Instantiate(treeModel, chunks[chunkIndex].gameObject.transform);
            tree.position = chunks[chunkIndex].vertices[vertexIndex];
            tree.rotation = Quaternion.LookRotation(chunks[chunkIndex].vertices[vertexIndex]);
            tree.Rotate(new Vector3(90, 0, 0));
        }

        private void PrepareChunk(ChunkData _chunkData, int _chunkTriangles)
        {
            var chunkObj = new GameObject();
            chunkObj.name = $"{chunkNameIndex++}";
            chunkObj.transform.SetParent(transform);
            chunkObj.AddComponent<MeshFilter>();
            chunkObj.AddComponent<MeshRenderer>();

            var chunk = new EdgeChunkRaw(chunkObj, _chunkData.vertices, _chunkTriangles, terrainSettings.material);
            _chunkData.chunk = chunk;

            chunks[chunksIndex++] = chunk;
        }
    }
}
