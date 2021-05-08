using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// reminder to add namespace when converting over
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EdgeChunk : MonoBehaviour
{
    public List<Vector3> vertices = new List<Vector3>();
    private int[] triangles;
    private Vector2[] uvs;
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

    public void Create(Vector3[] _vertices, Material _mat, int _subdivisions)
    {
        mat = _mat;
        subdivisions = Mathf.Max(0, _subdivisions);

        int triIndexCount = (int)Mathf.Pow(4, subdivisions) * 3;
        if (subdivisions == 0)
            triIndexCount = 3;

        triangles = new int[triIndexCount];

        // TODO: Make "List<Vector3>" vertices become "Vector3[] vertices"
        //var vertexCount = 1 + (2 + (int)Mathf.Pow(2, subdivisions) + 1) * ((int)Mathf.Pow(2, subdivisions)) / 2;

        vertices.AddRange(new List<Vector3> { _vertices[0], _vertices[1], _vertices[2] });

        // Create Edges
        edges = new Edge[3];
        CreateEdge(0, 1);
        CreateEdge(2, 1);
        CreateEdge(0, 2);

        // Create Inner Points
        CreateInnerPoints();

        CalculateUVs();

        // Triangles
        Triangulate();

        var noise = new Noise();

        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = vertices[i].normalized;
            vertices[i] = vertices[i] * Mathf.Max(0.5f, noise.Evaluate(vertices[i])) * 100f;
        }

        // Debug
        //Debug.DrawLine(vertices[0], vertices[1], Color.red, 10000);
        //Debug.DrawLine(vertices[1], vertices[2], Color.green, 10000);
        //Debug.DrawLine(vertices[2], vertices[0], Color.blue, 10000);
    }

    public void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.vertices = vertices.ToArray(); ;
        mesh.triangles = triangles.ToArray();
        //mesh.uv = uvs;
        mesh.normals = mesh.vertices.Select(s => s.normalized).ToArray();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().material = mat;
    }

    private void CalculateUVs()
    {
        uvs = new Vector2[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            uvs[i].x = (Mathf.Atan2(vertices[i].z, vertices[i].x) / (2f * Mathf.PI));
            uvs[i].y = (Mathf.Asin(vertices[i].y) / Mathf.PI) + 0.5f;
        }
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
            vertices.Add(vertex); // Add inner edge vertices to total array of chunk vertices
            innerEdgeIndices[i] = vertices.Count - 1; // For later reference when populating edgeIndices
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
                    vertices.Add(Vector3.Lerp(vertices[sideA.vertices[2 + i]], vertices[sideB.vertices[2 + i]], t));
                    row.AddTriangle(vertices.Count - 1);
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

    private static Dictionary<int, int> FindAndFixeWarpedFaces(ref int[] triangles, ref List<Vector3> vertices)
    {
        List<int> warpedFaces = new List<int>();
        Dictionary<int, int> checkedVert = new Dictionary<int, int>();

        // find warped faces
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 coordA = GetUvCoordinates(vertices[i]);
            Vector3 coordB = GetUvCoordinates(vertices[i + 1]);
            Vector3 coordC = GetUvCoordinates(vertices[i + 2]);

            Vector3 texNormal = Vector3.Cross(coordB - coordA, coordC - coordA);
            if (texNormal.z > 0)
            {
                warpedFaces.AddRange(new List<int>() { i, i + 1, i + 2 });
            }
        }

        // fix warped faces
        for (int i = 0; i < warpedFaces.Count; i += 3)
        {
            float xCoordA = GetUvCoordinates(vertices[warpedFaces[i]]).x;
            float xCoordB = GetUvCoordinates(vertices[warpedFaces[i + 1]]).x;
            float xCoordC = GetUvCoordinates(vertices[warpedFaces[i + 2]]).x;

            if (xCoordA < 0.25f)
            {
                int newIa = i;
                if (!checkedVert.TryGetValue(i, out newIa))
                {
                    vertices.Add(vertices[i]);
                    checkedVert[i] = vertices.Count - 1;
                    newIa = vertices.Count - 1;
                }
                warpedFaces[i] = newIa;
            }
            if (xCoordB < 0.25f)
            {
                int newIb = i + 1;
                if (!checkedVert.TryGetValue(i + 1, out newIb))
                {
                    vertices.Add(vertices[i + 1]);
                    checkedVert[i + 1] = vertices.Count - 1;
                    newIb = vertices.Count - 1;
                }
                warpedFaces[i + 1] = newIb;
            }
            if (xCoordC < 0.25f)
            {
                int newIc = i + 2;
                if (!checkedVert.TryGetValue(i + 2, out newIc))
                {
                    vertices.Add(vertices[i + 2]);
                    checkedVert[i + 2] = vertices.Count - 1;
                    newIc = vertices.Count - 1;
                }
                warpedFaces[i + 2] = newIc;
            }
        }

        return checkedVert;
    }

    //fix pole vertices incorrect U
    private static Dictionary<int, float> FindAndFixPoleVertices(ref List<int> triangles, ref List<Vector3> vertices)
    {
        float phi = (1f + Mathf.Sqrt(5f)) / 2f;

        Vector3 north = new Vector3(0f, phi, 0f);
        Vector3 south = -north;

        List<int> poleVerticeInd = new List<int>();
        Dictionary<int, float> poleVertIndicesCorrectU = new Dictionary<int, float>();

        for (int i = 0; i < triangles.Count; i++)
        {
            if (vertices[triangles[i]] == north || vertices[triangles[i]] == south)
            {
                if (!poleVerticeInd.Contains(triangles[i]))
                {
                    poleVerticeInd.Add(triangles[i]);
                }
                else
                {
                    vertices.Add(vertices[triangles[i]] == north ? north : south);
                    triangles[i] = vertices.Count - 1;
                }
                float xCoordB = GetUvCoordinates(vertices[triangles[i + 1]]).x;
                float xCoordC = GetUvCoordinates(vertices[triangles[i + 2]]).x;
                float correctedU = (xCoordB + xCoordC) / 2f + 0.5f; // I am not sure why it is needed but it seems needed...

                poleVertIndicesCorrectU[triangles[i]] = correctedU;
            }
        }

        return poleVertIndicesCorrectU;
    }

    private static Vector2 GetUvCoordinates(Vector3 vertice)
    {
        Vector3 vertCoord = vertice.normalized;
        float u = (Mathf.Atan2(vertCoord.z, vertCoord.x) / (2f * Mathf.PI));
        float v = (Mathf.Asin(vertCoord.y) / Mathf.PI) + 0.5f;

        return new Vector2(u, v);
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