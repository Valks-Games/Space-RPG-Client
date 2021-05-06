using SpaceGame.Utils;
using System.Collections.Generic;
using UnityEngine;

public class EdgeRenderer : MonoBehaviour
{
    public EdgeChunk[] chunks;
    public ChunkData[] chunkData;

    public Material mat;
    public int chunkSubdivisions;

    private readonly Vector3[] vertices = new Icosahedron().GetVertices();

    private readonly int[,] baseFormNeighbors = new int[20, 3]
    {
        { 1,  4, 6   },
        { 0,  2, 5   },
        { 1,  3, 9   },
        { 2,  4, 8   },
        { 0,  3, 7   },
        { 1,  15, 19 },
        { 0,  15, 16 },
        { 4,  16, 17 },
        { 3,  17, 18 },
        { 2,  18, 19 },
        { 11, 14, 15 },
        { 10, 12, 16 },
        { 11, 13, 17 },
        { 12, 14, 18 },
        { 10, 13, 19 },
        { 5,  6,  10 },
        { 6,  7,  11 },
        { 7,  8,  12 },
        { 8,  9,  13 },
        { 5,  9,  14 }
    };

    private int chunksIndex;
    private int chunkDataIndex;
    private int chunkNameIndex;

    private void Start()
    {
        var chunkCount = 20 * (int)Mathf.Pow(4, chunkSubdivisions);

        chunks = new EdgeChunk[chunkCount];
        chunkData = new ChunkData[chunkCount];

        InitializeChunks(new List<Vector3> { vertices[0], vertices[11], vertices[5] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[0], vertices[5], vertices[1] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[0], vertices[1], vertices[7] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[0], vertices[7], vertices[10] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[0], vertices[10], vertices[11] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[1], vertices[5], vertices[9] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[5], vertices[11], vertices[4] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[11], vertices[10], vertices[2] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[10], vertices[7], vertices[6] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[7], vertices[1], vertices[8] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[3], vertices[9], vertices[4] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[3], vertices[4], vertices[2] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[3], vertices[2], vertices[6] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[3], vertices[6], vertices[8] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[3], vertices[8], vertices[9] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[4], vertices[9], vertices[5] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[2], vertices[4], vertices[11] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[6], vertices[2], vertices[10] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[8], vertices[6], vertices[7] }, chunkSubdivisions);
        InitializeChunks(new List<Vector3> { vertices[9], vertices[8], vertices[1] }, chunkSubdivisions);

        var abc = true; // temp test value
        for (int i = 0; i < chunkData.Length; i++)
        {
            if (abc)
                GenerateChunk(chunkData[i], 0);
            else
                GenerateChunk(chunkData[i], 0);

            abc = !abc;
        }

        StitchEdges();

        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].GenerateMesh();
        }

        // For 3 subdivisions there are 64 vertices per base face

        // EDGE NEIGHBORS
        //var trianglesPerBaseFace = (int)Mathf.Pow(4, chunkSubdivisions);

        // First Edge
        var offset = 0;
        var currentPower = chunkSubdivisions - 1;
        for (int i = 0; i < Mathf.Pow(4, chunkSubdivisions); i+=4) 
        {
            if (i == (int)Mathf.Pow(4, currentPower)) 
            {
                offset = (int)Mathf.Pow(4, currentPower);
                currentPower++;
            }

            if (i < offset + 8 || (i >= offset + 16 && i < offset + 24)) // 3 chunkSubdivisions
            {
                chunkData[i].chunk.SetColor(Color.green);
                chunkData[i + 1].chunk.SetColor(Color.green);

                //chunkData[i + 256 + 64].chunk.SetColor(Color.red);
                //chunkData[i + 256 + 64 + 1].chunk.SetColor(Color.red);
            }
        }
    }

    private void StitchEdges()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            var redEdgeIndices = chunks[i].redEdge.vertices;
            var greenEdgeIndices = chunks[i].greenEdge.vertices;
            var blueEdgeIndices = chunks[i].blueEdge.vertices;

            var chunkVertices = chunks[i].vertices;

            var midpointBlue = chunkVertices[blueEdgeIndices[blueEdgeIndices.Length / 2]];
            var midpointGreen = chunkVertices[greenEdgeIndices[greenEdgeIndices.Length / 2]];
            var midpointRed = chunkVertices[redEdgeIndices[redEdgeIndices.Length / 2]];

            for (int k = 0; k < chunks.Length; k++)
            {
                var redEdgeIndicesOther = chunks[k].redEdge.vertices;
                var greenEdgeIndicesOther = chunks[k].greenEdge.vertices;
                var blueEdgeIndicesOther = chunks[k].blueEdge.vertices;

                var chunkVerticesOther = chunks[k].vertices;

                var midpointRedOther = chunkVerticesOther[redEdgeIndicesOther[redEdgeIndicesOther.Length / 2]];
                var midpointGreenOther = chunkVerticesOther[greenEdgeIndicesOther[greenEdgeIndicesOther.Length / 2]];
                var midpointBlueOther = chunkVerticesOther[blueEdgeIndicesOther[blueEdgeIndicesOther.Length / 2]];

                // Do not count itself (i != k)
                // If the neighboring chunk has a higher subdivision count then move that neighboring edge vertices to the chunk with lower subdivisions
                if (i != k && chunks[k].subdivisions > chunks[i].subdivisions)
                {
                    if (chunks[i].subdivisions == 0 || chunks[k].subdivisions == 0)
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
        var curChunk = chunks[curIndex];
        var neighborChunk = chunks[neighhorIndex];

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
                    if (neighborChunk.vertices[neighborEdgeVertices[neighborIndex]] != Vector3.Lerp(A, B, t))
                        neighborChunk.vertices[neighborEdgeVertices[neighborIndex]] = Vector3.Lerp(A, B, t);
                }
                else
                {
                    //new DebugPoint(neighborChunk.vertices[neighborEdgeVertices[neighborEdgeVertices.Length - 1 - neighborIndex]], "Purple").SetSize(0.4f).SetColor(Color.magenta);
                    //new DebugPoint(Vector3.Lerp(A, B, t), "Green").SetSize(0.4f).SetColor(Color.green);

                    if (neighborChunk.vertices[neighborEdgeVertices[neighborEdgeVertices.Length - 1 - neighborIndex]] != Vector3.Lerp(A, B, t))
                        neighborChunk.vertices[neighborEdgeVertices[neighborEdgeVertices.Length - 1 - neighborIndex]] = Vector3.Lerp(A, B, t);
                }

                neighborIndex++;
            }
        }
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
            InitializeChunk(_vertices[0], _vertices[3], _vertices[5]); // Top
            InitializeChunk(_vertices[5], _vertices[4], _vertices[2]); // Bottom Left
            InitializeChunk(_vertices[4], _vertices[5], _vertices[3]); // Bottom Middle
            InitializeChunk(_vertices[3], _vertices[1], _vertices[4]); // Bottom Right

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
        chunkData[chunkDataIndex++] = new ChunkData() { vertices = _vertices, center = chunkCenter };
    }

    private void GenerateChunk(ChunkData _chunkData, int _chunkTriangles)
    {
        var chunkObj = new GameObject();
        chunkObj.name = $"{chunkNameIndex++}";
        var chunk = chunkObj.AddComponent<EdgeChunk>();

        chunk.Create(_chunkData.vertices, mat, _chunkTriangles);
        _chunkData.chunk = chunk;

        chunks[chunksIndex++] = chunk;
    }

    public class ChunkData
    {
        public Vector3[] vertices;
        public Vector3 center;
        public EdgeChunk chunk;
    }

    public enum EdgeColor
    {
        Red,
        Green,
        Blue
    }
}