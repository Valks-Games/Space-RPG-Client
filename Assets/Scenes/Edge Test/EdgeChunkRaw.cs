using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SpaceGame.Celestial 
{
    public class EdgeChunkRaw
    {
        private readonly Material material;
        private readonly int[] triangles;

        public GameObject gameObject;
        public Mesh mesh;
        public Edge redEdge;
        public Edge greenEdge;
        public Edge blueEdge;
        public Edge[] edges;
        public Row[] rows;
        public Vector3[] vertices;
        public Color[] colors;
        public int subdivisions;

        private int lastEdgeVertex;
        private int edgeIndex;
        private int rowIndex;
        private int triIndex;
        private int vertexIndex;

        public EdgeChunkRaw(GameObject _gameObject, Vector3[] _vertices, int _subdivisions, Material _material)
        {
            gameObject = _gameObject;

            // SETUP MESH
            material = _material;
            subdivisions = Mathf.Max(0, _subdivisions);

            var vertexCount = 1 + (2 + (int)Mathf.Pow(2, subdivisions) + 1) * ((int)Mathf.Pow(2, subdivisions)) / 2;

            int triIndexCount = (int)Mathf.Pow(4, subdivisions) * 3;
            if (subdivisions == 0)
                triIndexCount = 3;

            triangles = new int[triIndexCount];
            vertices = new Vector3[vertexCount];
            colors = new Color[vertexCount];

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

            // Triangles
            Triangulate();
        }

        public void GenerateMesh()
        {
            mesh = new Mesh();
            mesh.name = "Chunk";
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors = colors;
            mesh.RecalculateNormals();
            gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            gameObject.GetComponent<MeshRenderer>().material = material;

            var collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }

        public void PerfectSphereNormals()
        {
            mesh.normals = mesh.vertices.Select(s => s.normalized).ToArray();
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
    }
}

