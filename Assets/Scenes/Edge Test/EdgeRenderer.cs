using SpaceGame.Utils;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SpaceGame.Celestial 
{
    public class EdgeRenderer : MonoBehaviour
    {
        public EdgePlanetSettings planetSettings;

        public Transform treeModel;
        public Transform grassModel;

        public EdgeChunk[] chunks;

        [Header("Planet Settings")]
        public NoiseSettings treeNoiseSettings;

        public float blendRange3;
        public float blendRange4;

        public bool planetSettingsFoldout;

        public INoiseFilter[] terrainNoise;

        public Noise noise = new Noise();

        private int chunksIndex;
        private int chunkNameIndex;

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

        public void GeneratePlanet()
        {
            ResetEverything();

            var subdividedIcosahedron = new IcosahedronSubdivided(planetSettings.chunkSubdivisions);

            chunksIndex = 0;
            chunkNameIndex = 0;

            chunks = new EdgeChunk[subdividedIcosahedron.chunkData.Length];

            // Prepare noise
            terrainNoise = new INoiseFilter[planetSettings.biomes.Length];
            for (int i = 0; i < planetSettings.biomes.Length; i++)
            {
                terrainNoise[i] = NoiseFilterFactory.CreateNoiseFilter(planetSettings.biomes[i].terrainNoise);
            }

            // Populate chunks array
            var abc = true;
            for (int i = 0; i < subdividedIcosahedron.chunkData.Length; i++)
            {
                if (abc)
                    GenerateChunk(subdividedIcosahedron.chunkData[i], planetSettings.chunkTriangleSubdivisions);
                else
                    GenerateChunk(subdividedIcosahedron.chunkData[i], planetSettings.chunkTriangleSubdivisions);

                abc = !abc;
            }

            IcosahedronSubdivided.StitchEdges(chunks);

            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i].GenerateMesh();
            }

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

                        if ((chunks[i].vertices[j] - new Vector3(0, 0, 0)).magnitude > planetSettings.radius + 0.25f) // Don't spawn in the water
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
                chunks[i].PerfectSphereNormals();
            }

            /*ChunkData[] oceanChunkData;

            // Ocean
            var ocean = new IcosahedronSubdivided(1);
            oceanChunkData = ocean.chunkData;

            for (int i = 0; i < oceanChunkData.Length; i++) 
            {
                var chunkObj = new GameObject();
                chunkObj.name = $"{chunkNameIndex++}";
                chunkObj.transform.SetParent(transform);
                chunkObj.AddComponent<MeshFilter>();
                chunkObj.AddComponent<MeshRenderer>();

                var chunk = new EdgeChunk(chunkObj, oceanChunkData[i].vertices, this, 3);
                chunk.GenerateMesh();
                oceanChunkData[i].chunk = chunk;
            }*/
        }

        private void SpawnTree(int chunkIndex, int vertexIndex)
        {
            var tree = Instantiate(treeModel, chunks[chunkIndex].gameObject.transform);
            tree.position = chunks[chunkIndex].vertices[vertexIndex];
            tree.rotation = Quaternion.LookRotation(chunks[chunkIndex].vertices[vertexIndex]);
            tree.Rotate(new Vector3(90, 0, 0));
        }

        private void GenerateChunk(ChunkData _chunkData, int _chunkTriangles)
        {
            var chunkObj = new GameObject();
            chunkObj.name = $"{chunkNameIndex++}";
            chunkObj.transform.SetParent(transform);
            chunkObj.AddComponent<MeshFilter>();
            chunkObj.AddComponent<MeshRenderer>();

            var chunk = new EdgeChunk(chunkObj, _chunkData.vertices, this, _chunkTriangles);
            _chunkData.chunk = chunk;

            chunks[chunksIndex++] = chunk;
        }
    }
}
