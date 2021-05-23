using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SpaceGame.Celestial;
using SpaceGame.Utils;

// reminder to add namespace when converting over
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EdgeChunk : MonoBehaviour
{
    public Vector3[] vertices;
    private int[] triangles;
    private Vector4[] uvs;
    public Edge[] edges;
    private Row[] rows;

    public int subdivisions;

    private Mesh mesh;

    private Material mat;

    public Edge redEdge;
    public Edge greenEdge;
    public Edge blueEdge;
    private int lastEdgeVertex;
    private int edgeIndex;
    private int rowIndex;
    private int triIndex;
    private int vertexIndex;

    private EdgeRenderer edgeRenderer;

    public void Create(Vector3[] _vertices, EdgeRenderer _edgeRenderer, int _subdivisions)
    {
        edgeRenderer = _edgeRenderer;
        mat = edgeRenderer.mat;
        subdivisions = Mathf.Max(0, _subdivisions);

        var vertexCount = 1 + (2 + (int)Mathf.Pow(2, subdivisions) + 1) * ((int)Mathf.Pow(2, subdivisions)) / 2;

        int triIndexCount = (int)Mathf.Pow(4, subdivisions) * 3;
        if (subdivisions == 0)
            triIndexCount = 3;

        triangles = new int[triIndexCount];


        // TODO: Make "List<Vector3>" vertices become "Vector3[] vertices"

        vertices = new Vector3[vertexCount];
        uvs = new Vector4[vertexCount];

        UpdateUVs();

        vertices[vertexIndex++] = _vertices[0];
        vertices[vertexIndex++] = _vertices[1];
        vertices[vertexIndex++] = _vertices[2];

        // Create Edges
        edges = new Edge[3];
        CreateEdge(0, 1);
        CreateEdge(2, 1);
        CreateEdge(0, 2);

        // Create Inner Points
        CreateInnerPoints();


        //CalculateUVs();

        // Triangles
        Triangulate();

        var terrainSettings1 = NoiseFilterFactory.CreateNoiseFilter(edgeRenderer.terrainSettings1);
        var terrainSettings2 = NoiseFilterFactory.CreateNoiseFilter(edgeRenderer.terrainSettings2);
        var terrainSettings3 = NoiseFilterFactory.CreateNoiseFilter(edgeRenderer.terrainSettings3);

        var blendRange = edgeRenderer.blendRange;
        var blendRange2 = edgeRenderer.blendRange2;
        //edgeRenderer.biomeBlendLine1.simpleNoiseSettings.minValue = -blendLine;

        var biomeBlendLine1 = NoiseFilterFactory.CreateNoiseFilter(edgeRenderer.biomeBlendLine1);
        var biome1 = NoiseFilterFactory.CreateNoiseFilter(edgeRenderer.biome1);

        for (int i = 0; i < vertices.Length; i++) 
        {
            /*var biomeBlendLine1Noise = biomeBlendLine1.Evaluate(vertices[i]);
            var biomeNoise = biome1.Evaluate(vertices[i]);

            var blueRedNoise = 0f;
            var blueNoise = 0f;
            var redNoise = 0f;
            var greenNoise = 0f;
            var redGreenNoise = 0f;

            var biomeLine = blendRange + biomeBlendLine1Noise;

            if (biomeNoise < -biomeLine + offset - 0.4f && biomeNoise >= -biomeLine + offset - 0.5f)
            {
                uvs[i] = Color.black;
            }

            if (biomeNoise < -biomeLine + offset - 0.3f && biomeNoise >= -biomeLine + offset - 0.4f)
            {
                uvs[i] = Color.cyan;
            }

            if (biomeNoise < -biomeLine + offset - 0.2f && biomeNoise >= -biomeLine + offset - 0.3f)
            {
                uvs[i] = Color.blue;
            }

            if (biomeNoise <= -biomeLine + blendRange2 + offset - 0.1f && biomeNoise >= -biomeLine - blendRange2 + offset + 0.2f) 
            {
                uvs[i] = Color.magenta;
            }

            if (biomeNoise < -biomeLine + offset && biomeNoise > -biomeLine + offset - 0.1f) 
            {
                redNoise = terrainSettings1.Evaluate(vertices[i]);
                uvs[i] = Color.red;
            }

            if (biomeNoise >= -biomeLine + offset && biomeNoise < biomeLine + offset) 
            {
                //var t = Remap(biomeNoise, -biomeLine, biomeLine, 0, 1);
                //redGreenNoise = Mathf.Lerp(terrainSettings1.Evaluate(vertices[i]), terrainSettings2.Evaluate(vertices[i]), t);
                //uvs[i] = Color.Lerp(Color.red, Color.green, t);
                uvs[i] = Color.yellow;
            }

            if (biomeNoise >= biomeLine + offset) 
            {
                greenNoise = terrainSettings2.Evaluate(vertices[i]);
                uvs[i] = Color.green;
            }

            var totalNoise = redNoise + redGreenNoise + greenNoise + blueNoise + blueRedNoise;
            vertices[i] = vertices[i].normalized * (edgeRenderer.radius + totalNoise);*/


            vertices[i] = vertices[i].normalized;
            EvaluateBiome(i, vertices[i].y, 4);


            
        }

        // Debug
        //Debug.DrawLine(vertices[0], vertices[1], Color.red, 10000);
        //Debug.DrawLine(vertices[1], vertices[2], Color.green, 10000);
        //Debug.DrawLine(vertices[2], vertices[0], Color.blue, 10000);

        transform.SetParent(GameObject.Find("Renderer").transform);
        //gameObject.isStatic = true;
    }

    public void EvaluateBiome(int vertexIndex, float y, int numBiomes)
    {
        var biomeColors = new Color[] { Color.red, Color.green, Color.blue, Color.magenta, Color.black };

        var height = Remap(y, -1, 1, 0, 0.99f);
        var biomeIndex = Mathf.FloorToInt(height * (numBiomes));

        uvs[vertexIndex] = biomeColors[biomeIndex]; // Biome Colors

        var biome = height * numBiomes;

        // Blending Zones
        /*for (int i = 1; i < numBiomes; i++) 
        {
            if (biome > i - edgeRenderer.blendRange && biome < i + edgeRenderer.blendRange)
            {
                var t = Remap(y, -edgeRenderer.blendRange, edgeRenderer.blendRange, 0, 1);
                uvs[vertexIndex] = Color.Lerp(biomeColors[i - 1], biomeColors[i], t);
            }
        }*/

        for (int i = 1; i < numBiomes; i++) 
        {
            if (biome > i - edgeRenderer.blendRange && biome < i + edgeRenderer.blendRange)
            {
                var pivot = -1 + ((float)i / numBiomes) * 2;
                var t = Remap(y, pivot - edgeRenderer.blendRange / numBiomes, pivot + edgeRenderer.blendRange / numBiomes, 0, 1);

                uvs[vertexIndex] = Color.Lerp(biomeColors[i - 1], biomeColors[i], t);
            }
        }
    }

    public void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.name = "Chunk";
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        //UpdateUVs();
        mesh.SetUVs(0, uvs);
        mesh.normals = mesh.vertices.Select(s => s.normalized).ToArray();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().material = mat;

        //var collider = gameObject.AddComponent<MeshCollider>();
        //collider.sharedMesh = mesh;
    }

    public void UpdateUVs() 
    {
        
    }

    private float Remap(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    private void Triangulate()
    {
        redEdge = edges[0];
        greenEdge = edges[1];
        blueEdge = edges[2];
        lastEdgeVertex = edges[0].vertices.Length - 1; // All edges have the same vertex count

        if (subdivisions == 0)
        {
            Triangle(0, 1, 2);
            return;
        }

        // TRIANGLES WITH NO PATTERNS
        // Top Triangle
        Triangle(blueEdge.vertices[1], 0, redEdge.vertices[1]);

        // First Triangle from Bottom Left
        Triangle(blueEdge.vertices[lastEdgeVertex], blueEdge.vertices[lastEdgeVertex - 1], greenEdge.vertices[1]);
        // First Triangle from Bottom Right
        Triangle(greenEdge.vertices[lastEdgeVertex - 1], redEdge.vertices[lastEdgeVertex - 1], redEdge.vertices[lastEdgeVertex]);

        if (subdivisions == 1)
            Triangle(greenEdge.vertices[1], blueEdge.vertices[1], redEdge.vertices[1]);

        if (subdivisions < 2)
            return;

        // Tri just below top tri
        Triangle(rows[0].vertices[0], blueEdge.vertices[1], redEdge.vertices[1]); // Not included with pattern because of redEdge

        // First Inner Row Triangle
        Triangle(rows[1].vertices[0], rows[0].vertices[0], rows[1].vertices[1]);

        // Second Triangle from Bottom Left
        Triangle(greenEdge.vertices[1], blueEdge.vertices[lastEdgeVertex - 1], rows[rows.Length - 1].vertices[0]); // Not included with pattern because of blueEdge
        // Second Triangle from Bottom Right
        Triangle(greenEdge.vertices[lastEdgeVertex - 1], rows[rows.Length - 1].vertices[rows[rows.Length - 1].vertices.Count - 1], redEdge.vertices[lastEdgeVertex - 1]); // Not included with pattern because of redEdge

        // TRIANGLES WITH PATTERNS
        BottomRowTriangles();
        LeftRowTriangles();
        RightRowTriangles();
        InnerRowTriangles();
    }

    private void LeftRowTriangles()
    {
        for (int i = 0; i < blueEdge.vertices.Length - 3; i++)
            Triangle(blueEdge.vertices[2 + i], blueEdge.vertices[1 + i], rows[i].vertices[0]); // 1st tri top to bottom

        for (int i = 0; i < blueEdge.vertices.Length - 4; i++)
            Triangle(rows[i + 1].vertices[0], blueEdge.vertices[2 + i], rows[i].vertices[0]); // 2nd tri top to bottom
    }

    private void RightRowTriangles()
    {
        for (int i = 0; i < redEdge.vertices.Length - 3; i++) // Upside Triangles
            Triangle(rows[i].vertices[rows[i].vertices.Count - 1], redEdge.vertices[1 + i], redEdge.vertices[2 + i]);

        for (int i = 0; i < redEdge.vertices.Length - 4; i++) // Upside Down Triangles
            Triangle(rows[i].vertices[rows[i].vertices.Count - 1], redEdge.vertices[2 + i], rows[i + 1].vertices[rows[i + 1].vertices.Count - 1]);
    }

    private void BottomRowTriangles()
    {
        // Add triangles from left to right filling in middle
        for (int i = 0; i < rows[rows.Length - 1].vertices.Count; i++) // Upside Triangles
            Triangle(greenEdge.vertices[1 + i], rows[rows.Length - 1].vertices[i], greenEdge.vertices[2 + i]);

        for (int i = 0; i < rows[rows.Length - 1].vertices.Count - 1; i++) // Upside Down Triangles
            Triangle(greenEdge.vertices[i + 2], rows[rows.Length - 1].vertices[i], rows[rows.Length - 1].vertices[i + 1]);
    }

    private void InnerRowTriangles()
    {
        // Second Row and beyond
        for (int r = 1; r < rows.Length - 1; r++)
        {
            for (int i = 0; i < rows[r].vertices.Count; i++) // Upside Triangles
                Triangle(rows[r + 1].vertices[i], rows[r].vertices[i], rows[r + 1].vertices[1 + i]);

            for (int i = 0; i < rows[r].vertices.Count - 1; i++) // Upside Down Triangles
                Triangle(rows[r + 1].vertices[1 + i], rows[r].vertices[i], rows[r].vertices[1 + i]);
        }
    }

    /*!
     * Creates a edge with a start vertex, end vertex and the inner vertices also
     * known as the number of inner edge divisions.
     */

    private void CreateEdge(int start, int end)
    {
        var divisions = Mathf.Max(0, (int)Mathf.Pow(2, subdivisions) - 1);
        var innerEdgeIndices = new int[divisions];

        for (int i = 0; i < divisions; i++)
        {
            float t = (i + 1f) / (divisions + 1f);
            var vertex = Vector3.Lerp(vertices[start], vertices[end], t); // Calculate inner vertices
            vertices[vertexIndex++] = (vertex); // Add inner edge vertices to total array of chunk vertices
            innerEdgeIndices[i] = vertexIndex - 1; // For later reference when populating edgeIndices
        }

        // Populate edge indices for later reference
        var edgeIndicies = new int[divisions + 2]; // Edge indicies include start + end + inner indices

        edgeIndicies[0] = start; // Populate start vertex

        for (int i = 0; i < divisions; i++) // Populate inner vertices
            edgeIndicies[i + 1] = innerEdgeIndices[i];

        edgeIndicies[edgeIndicies.Length - 1] = end; // Populate end vertex

        edges[edgeIndex++] = new Edge(edgeIndicies);
    }

    /*!
     * Creates the vertices inside the triangle that do not touch any outside edge.
     */

    private void CreateInnerPoints()
    {
        if (subdivisions > 1)
        {
            var numRows = edges[0].vertices.Length - 3;
            rows = new Row[numRows];
            for (int i = 0; i < numRows; i++)
            {
                var sideA = edges[2]; // Vertices in sideA created from bottom to top
                var sideB = edges[0]; // Vertices in sideB created from top to bottom

                var row = new Row();
                var numColumns = i + 1;
                for (int j = 0; j < numColumns; j++)
                {
                    var t = (j + 1f) / (numColumns + 1f);

                    // Create inner point
                    // [sideA.vertexIndices.Length - 3 - i] We subtract 3 to skip over "end" vertex and the first row.
                    // [2 + i] to skip over "start" vertex and the first row.
                    vertices[vertexIndex++] = (Vector3.Lerp(vertices[sideA.vertices[2 + i]], vertices[sideB.vertices[2 + i]], t));
                    row.AddTriangle(vertexIndex - 1);
                }
                rows[rowIndex++] = row;
            }
        }
    }

    private void Triangle(int a, int b, int c)
    {
        triangles[triIndex++] = a;
        triangles[triIndex++] = b;
        triangles[triIndex++] = c;
    }

    public void SetColor(Color color)
    {
        GetComponent<MeshRenderer>().material.SetColor("_Color", color);
    }

    private void OnDrawGizmos()
    {
        /*Gizmos.color = Color.red;
        Gizmos.DrawSphere(vertices[0], 0.05f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(vertices[1], 0.05f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(vertices[2], 0.05f);

        Gizmos.color = Color.yellow;
        for (int i = 3; i < vertices.Count; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.01f);
        }*/
    }
}

/*!
 * A Edge counts both the start and end vertex as well as all the vertices in between.
 */

public class Edge
{
    public int[] vertices; // Referenced by index in EdgeChunk.vertices

    public Edge(int[] _vertices)
    {
        vertices = _vertices;
    }
}

/*!
 * A Row does not count the outer vertices touching the outer edges.
 */

public class Row
{
    public List<int> vertices = new List<int>(); // Referenced by index in EdgeChunk.vertices

    public void AddTriangle(int _vertex)
    {
        vertices.Add(_vertex);
    }
}