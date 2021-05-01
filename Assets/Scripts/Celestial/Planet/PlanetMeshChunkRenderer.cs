using SpaceGame.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame.Celestial
{
    public class PlanetMeshChunkRenderer
    {
        private Vector3[] baseFormVertices;

        public ShapeGenerator shapeGenerator;
        public ColourGenerator colourGenerator;

        public ShapeSettings shapeSettings;

        private Transform parent;
        private Planet planet;

        public Noise noise = new Noise();

        private GameObject test = GameObject.Find("Render Debug Point"); // Render everything with respect to this;

        public enum ShapeType { Noise, Sphere }

        public ShapeType shapeType;

        public ChunkData[] chunkData;
        private int chunkDataIndex;
        private int chunkCount;

        public class ChunkData 
        {
            public Vector3[] vertices; // the 3 vertices that make up the chunk
            public Vector3 center;
            public PlanetMeshChunk chunk;
        }

        public PlanetMeshChunkRenderer(Planet _planet, Transform _parent, ShapeGenerator _shapeGenerator, ColourGenerator _colourGenerator, ShapeType _shapeType)
        {
            planet = _planet;
            shapeType = _shapeType;

            parent = _parent;
            shapeGenerator = _shapeGenerator;
            shapeSettings = _shapeGenerator.shapeSettings;
            colourGenerator = _colourGenerator;

            if (shapeType == ShapeType.Noise)
                chunkCount = 20 * (int)Mathf.Pow(4, shapeSettings.chunks);
            else
                chunkCount = 20 * (int)Mathf.Pow(4, shapeSettings.oceanChunks);

            chunkData = new ChunkData[chunkCount];

            baseFormVertices = new Icosahedron(shapeSettings.radius).GetVertices();

            Initialize();

            PlanetMeshChunk.count = 0; // To make the counting nicer in game object hiearchy
        }

        private void Initialize() 
        {
            var chunks = shapeSettings.chunks; // Terrain Chunk Subdivisions

            if (shapeType == ShapeType.Sphere)
                chunks = shapeSettings.oceanChunks; // Ocean Chunk Subdivisions

            InitializeChunks(new List<Vector3> { baseFormVertices[0], baseFormVertices[11], baseFormVertices[5] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[0], baseFormVertices[5], baseFormVertices[1] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[0], baseFormVertices[1], baseFormVertices[7] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[0], baseFormVertices[7], baseFormVertices[10] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[0], baseFormVertices[10], baseFormVertices[11] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[1], baseFormVertices[5], baseFormVertices[9] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[5], baseFormVertices[11], baseFormVertices[4] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[11], baseFormVertices[10], baseFormVertices[2] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[10], baseFormVertices[7], baseFormVertices[6] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[7], baseFormVertices[1], baseFormVertices[8] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[3], baseFormVertices[9], baseFormVertices[4] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[3], baseFormVertices[4], baseFormVertices[2] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[3], baseFormVertices[2], baseFormVertices[6] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[3], baseFormVertices[6], baseFormVertices[8] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[3], baseFormVertices[8], baseFormVertices[9] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[4], baseFormVertices[9], baseFormVertices[5] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[2], baseFormVertices[4], baseFormVertices[11] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[6], baseFormVertices[2], baseFormVertices[10] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[8], baseFormVertices[6], baseFormVertices[7] }, chunks);
            InitializeChunks(new List<Vector3> { baseFormVertices[9], baseFormVertices[8], baseFormVertices[1] }, chunks);
        }

        private void InitializeChunks(List<Vector3> _vertices, int n)
        {
            // No subdivisions
            if (n <= 0) 
            {
                InitializeChunk(_vertices[0], _vertices[1], _vertices[2]);

                return;
            }

            _vertices.Add(SphereUtils.GetMidPointVertex(_vertices[0], _vertices[1])); // Right Middle (3)
            _vertices.Add(SphereUtils.GetMidPointVertex(_vertices[1], _vertices[2])); // Bottom Middle (4)
            _vertices.Add(SphereUtils.GetMidPointVertex(_vertices[2], _vertices[0])); // Left middle (5)

            // Only draw the last recursion
            if (n == 1)
            {
                InitializeChunk( _vertices[0], _vertices[3], _vertices[5]); // Top
                InitializeChunk( _vertices[5], _vertices[4], _vertices[2]); // Bottom Left
                InitializeChunk( _vertices[4], _vertices[5], _vertices[3]); // Bottom Middle
                InitializeChunk( _vertices[3], _vertices[1], _vertices[4]); // Bottom Right

                return;
            }

            InitializeChunks(new List<Vector3> { _vertices[0], _vertices[3], _vertices[5] }, n - 1); // Top
            InitializeChunks(new List<Vector3> { _vertices[5], _vertices[4], _vertices[2] }, n - 1); // Bottom Left
            InitializeChunks(new List<Vector3> { _vertices[4], _vertices[5], _vertices[3] }, n - 1); // Bottom Middle
            InitializeChunks(new List<Vector3> { _vertices[3], _vertices[1], _vertices[4] }, n - 1); // Bottom Right
        }

        private void InitializeChunk(params Vector3[] _vertices)
        {
            var chunkCenter = SphereUtils.GetCenterPoint(_vertices[0], _vertices[1], _vertices[2]);
            chunkData[chunkDataIndex++] = (new ChunkData() { vertices = _vertices, center = chunkCenter });
        }

        private void GenerateChunk(ChunkData _chunkData, int _chunkTriangles)
        {
            planet.transform.position = Vector3.zero;

            // Create chunk gameObject
            var chunkObj = new GameObject();

            // Set parent
            chunkObj.transform.parent = parent.transform;

            // Add PlanetMeshChunk script to chunk gameObject
            var chunk = chunkObj.AddComponent<PlanetMeshChunk>();

            chunk.Create(this, _chunkData.vertices, _chunkTriangles);
            _chunkData.chunk = chunk;

            chunk.GenerateMesh();

            planet.transform.localPosition = planet.curLocalPos;
        }

        public void ReRenderAllChunks() 
        {
            for (int i = 0; i < chunkData.Length; i++) 
            {
                if (chunkData[i].chunk)
                    chunkData[i].chunk.GenerateMesh();
            }
        }

        public void RenderNearbyChunks(float _distance)
        {
            if (chunkData == null || chunkData.Length == 0)
                return;

            var maxDist = 5000;

            // Check which chunks to render
            for (int i = 0; i < chunkData.Length; i++)
            {
                var curDist = Vector3.Distance(planet.curLocalPos + chunkData[i].center, test.transform.localPosition);
                if ((curDist < maxDist) || shapeSettings.renderEverything)
                {
                    // Create chunk
                    if (!chunkData[i].chunk)
                    {
                        var LOD = Mathf.Max(1, (int)(maxDist - curDist) / 700);
                        GenerateChunk(chunkData[i], LOD);

                        StitchEdges();
                        ReRenderAllChunks();
                    }
                }
                else
                {
                    // Destroy chunk
                    if (chunkData[i].chunk)
                    {
                        Object.DestroyImmediate(chunkData[i].chunk.gameObject);
                        chunkData[i].chunk = null;
                    }
                }
            }
        }

        public void StitchEdges()
        {
            for (int i = 0; i < chunkData.Length; i++)
            {
                if (!chunkData[i].chunk) continue;
                var redEdgeIndices = chunkData[i].chunk.redEdge.vertices;
                var greenEdgeIndices = chunkData[i].chunk.greenEdge.vertices;
                var blueEdgeIndices = chunkData[i].chunk.blueEdge.vertices;

                var chunkVertices = chunkData[i].chunk.vertices;

                var midpointBlue = chunkVertices[blueEdgeIndices[blueEdgeIndices.Length / 2]];
                var midpointGreen = chunkVertices[greenEdgeIndices[greenEdgeIndices.Length / 2]];
                var midpointRed = chunkVertices[redEdgeIndices[redEdgeIndices.Length / 2]];

                for (int k = 0; k < chunkData.Length; k++)
                {
                    if (!chunkData[k].chunk) continue;
                    var redEdgeIndicesOther = chunkData[k].chunk.redEdge.vertices;
                    var greenEdgeIndicesOther = chunkData[k].chunk.greenEdge.vertices;
                    var blueEdgeIndicesOther = chunkData[k].chunk.blueEdge.vertices;

                    var chunkVerticesOther = chunkData[k].chunk.vertices;

                    var midpointRedOther = chunkVerticesOther[redEdgeIndicesOther[redEdgeIndicesOther.Length / 2]];
                    var midpointGreenOther = chunkVerticesOther[greenEdgeIndicesOther[greenEdgeIndicesOther.Length / 2]];
                    var midpointBlueOther = chunkVerticesOther[blueEdgeIndicesOther[blueEdgeIndicesOther.Length / 2]];

                    // Do not count itself (i != k)
                    // If the neighboring chunk has a higher subdivision count then move that neighboring edge vertices to the chunk with lower subdivisions
                    if (i != k && chunkData[k].chunk.subdivisions > chunkData[i].chunk.subdivisions)
                    {
                        if (chunkData[i].chunk.subdivisions == 0 || chunkData[k].chunk.subdivisions == 0)
                        {
                            // RR GG BB (For some reason when the edge colors are the same the stitching needs to be done in reverse)
                            // RR
                            var RR_L = chunkVertices[redEdgeIndices[0]] == chunkVerticesOther[redEdgeIndicesOther[redEdgeIndicesOther.Length - 1]];
                            var RR_R = chunkVertices[redEdgeIndices[redEdgeIndices.Length - 1]] == chunkVerticesOther[redEdgeIndicesOther[0]];

                            if (RR_L && RR_R)
                            {
                                StitchChunkEdges(EdgeColor.Red, EdgeColor.Red, i, k);
                            }

                            // GG
                            var GG_L = chunkVertices[greenEdgeIndices[0]] == chunkVerticesOther[greenEdgeIndicesOther[greenEdgeIndicesOther.Length - 1]];
                            var GG_R = chunkVertices[greenEdgeIndices[greenEdgeIndices.Length - 1]] == chunkVerticesOther[greenEdgeIndicesOther[0]];

                            if (GG_L && GG_R)
                            {
                                StitchChunkEdges(EdgeColor.Green, EdgeColor.Green, i, k);
                            }

                            // BB
                            var BB_L = chunkVertices[blueEdgeIndices[0]] == chunkVerticesOther[blueEdgeIndicesOther[blueEdgeIndicesOther.Length - 1]];
                            var BB_R = chunkVertices[blueEdgeIndices[blueEdgeIndices.Length - 1]] == chunkVerticesOther[blueEdgeIndicesOther[0]];

                            if (BB_L && BB_R)
                            {
                                StitchChunkEdges(EdgeColor.Blue, EdgeColor.Blue, i, k);
                            }

                            // RB
                            var RB_L = chunkVertices[redEdgeIndices[0]] == chunkVerticesOther[blueEdgeIndicesOther[0]];
                            var RB_R = chunkVertices[redEdgeIndices[redEdgeIndices.Length - 1]] == chunkVerticesOther[blueEdgeIndicesOther[blueEdgeIndicesOther.Length - 1]];

                            if (RB_L && RB_R)
                            {
                                StitchChunkEdges(EdgeColor.Red, EdgeColor.Blue, i, k);
                            }

                            // RG
                            var RG_L = chunkVertices[redEdgeIndices[0]] == chunkVerticesOther[greenEdgeIndicesOther[0]];
                            var RG_R = chunkVertices[redEdgeIndices[redEdgeIndices.Length - 1]] == chunkVerticesOther[greenEdgeIndicesOther[greenEdgeIndicesOther.Length - 1]];

                            if (RG_L && RG_R)
                            {
                                StitchChunkEdges(EdgeColor.Red, EdgeColor.Green, i, k);
                            }

                            // BR
                            var BR_L = chunkVertices[blueEdgeIndices[0]] == chunkVerticesOther[redEdgeIndicesOther[0]];
                            var BR_R = chunkVertices[blueEdgeIndices[blueEdgeIndices.Length - 1]] == chunkVerticesOther[redEdgeIndicesOther[redEdgeIndicesOther.Length - 1]];

                            if (BR_L && BR_R)
                            {
                                StitchChunkEdges(EdgeColor.Blue, EdgeColor.Red, i, k);
                            }

                            // BG
                            var BG_L = chunkVertices[blueEdgeIndices[0]] == chunkVerticesOther[greenEdgeIndicesOther[0]];
                            var BG_R = chunkVertices[blueEdgeIndices[blueEdgeIndices.Length - 1]] == chunkVerticesOther[greenEdgeIndicesOther[greenEdgeIndicesOther.Length - 1]];

                            if (BG_L && BG_R)
                            {
                                StitchChunkEdges(EdgeColor.Blue, EdgeColor.Green, i, k);
                            }

                            // GR
                            var GR_L = chunkVertices[greenEdgeIndices[0]] == chunkVerticesOther[redEdgeIndicesOther[0]];
                            var GR_R = chunkVertices[greenEdgeIndices[greenEdgeIndices.Length - 1]] == chunkVerticesOther[redEdgeIndicesOther[redEdgeIndicesOther.Length - 1]];

                            if (GR_L && GR_R)
                            {
                                StitchChunkEdges(EdgeColor.Green, EdgeColor.Red, i, k);
                            }

                            // GB
                            var GB_L = chunkVertices[greenEdgeIndices[0]] == chunkVerticesOther[blueEdgeIndicesOther[0]];
                            var GB_R = chunkVertices[greenEdgeIndices[greenEdgeIndices.Length - 1]] == chunkVerticesOther[blueEdgeIndicesOther[blueEdgeIndicesOther.Length - 1]];

                            if (GB_L && GB_R)
                            {
                                StitchChunkEdges(EdgeColor.Green, EdgeColor.Blue, i, k);
                            }
                        }
                        else
                        {
                            // RR GG BB (For some reason when the edge colors are the same the stitching needs to be done in reverse)
                            if (midpointRed == midpointRedOther)
                                StitchChunkEdges(EdgeColor.Red, EdgeColor.Red, i, k);

                            if (midpointGreen == midpointGreenOther)
                                StitchChunkEdges(EdgeColor.Green, EdgeColor.Green, i, k);

                            if (midpointBlue == midpointBlueOther)
                                StitchChunkEdges(EdgeColor.Blue, EdgeColor.Blue, i, k);

                            // RB RG
                            if (midpointRed == midpointBlueOther)
                                StitchChunkEdges(EdgeColor.Red, EdgeColor.Blue, i, k);

                            if (midpointRed == midpointGreenOther)
                                StitchChunkEdges(EdgeColor.Red, EdgeColor.Green, i, k);

                            // BR BG
                            if (midpointBlue == midpointRedOther)
                                StitchChunkEdges(EdgeColor.Blue, EdgeColor.Red, i, k);

                            if (midpointBlue == midpointGreenOther)
                                StitchChunkEdges(EdgeColor.Blue, EdgeColor.Green, i, k);

                            // GR GB
                            if (midpointGreen == midpointRedOther)
                                StitchChunkEdges(EdgeColor.Green, EdgeColor.Red, i, k);

                            if (midpointGreen == midpointBlueOther)
                                StitchChunkEdges(EdgeColor.Green, EdgeColor.Blue, i, k);
                        }
                    }
                }
            }
        }

        private void StitchChunkEdges(EdgeColor edgeCurrent, EdgeColor edgeNeighbor, int curIndex, int neighhorIndex)
        {
            var curChunk = chunkData[curIndex].chunk;
            var neighborChunk = chunkData[neighhorIndex].chunk;

            var neighborEdgeVertices = neighborChunk.edges[(int)edgeNeighbor].vertices;
            var neighborIndex = 0;

            for (int k = 0; k < curChunk.edges[(int)edgeCurrent].vertices.Length - 1; k++)
            {
                var A = curChunk.vertices[curChunk.edges[(int)edgeCurrent].vertices[k]];
                var B = curChunk.vertices[curChunk.edges[(int)edgeCurrent].vertices[k + 1]];

                // Get curChunk points
                int midpointCount = (int)Mathf.Pow(2, neighborChunk.subdivisions - curChunk.subdivisions);

                neighborIndex++;

                for (int i = 1; i < midpointCount; i++)
                {
                    float t = i / (float)midpointCount;

                    if (edgeCurrent != edgeNeighbor)
                    {
                        //new DebugPoint(neighborChunk.vertices[neighborEdgeVertices[neighborIndex]], "Purple").SetSize(20f).SetColor(Color.magenta);
                        //new DebugPoint(Vector3.Lerp(A, B, t), "Green").SetSize(20f).SetColor(Color.green);

                        if (neighborChunk.vertices[neighborEdgeVertices[neighborIndex]] != Vector3.Lerp(A, B, t))
                            neighborChunk.vertices[neighborEdgeVertices[neighborIndex]] = Vector3.Lerp(A, B, t);
                    }
                    else
                    {
                        if (neighborChunk.vertices[neighborEdgeVertices[neighborEdgeVertices.Length - 1 - neighborIndex]] != Vector3.Lerp(A, B, t))
                            neighborChunk.vertices[neighborEdgeVertices[neighborEdgeVertices.Length - 1 - neighborIndex]] = Vector3.Lerp(A, B, t);
                    }

                    neighborIndex++;
                }
            }
        }

        public enum EdgeColor
        {
            Red,
            Green,
            Blue
        }
    }
}