using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SpaceGame.Utils;

namespace SpaceGame.Celestial 
{
    public class EdgeChunk
    {
        private readonly EdgePlanetSettings planetSettings;
        private readonly EdgeRenderer edgeRenderer;
        private readonly Material material;
        private readonly int[] triangles;
        private readonly Color[] colors;
        private readonly INoiseFilter biome;
        private readonly INoiseFilter biomeBlendLine;

        public GameObject gameObject;
        public Mesh mesh;
        public Edge redEdge;
        public Edge greenEdge;
        public Edge blueEdge;
        public Edge[] edges;
        public Row[] rows;
        public Vector3[] vertices;
        public int subdivisions;

        private int lastEdgeVertex;
        private int edgeIndex;
        private int rowIndex;
        private int triIndex;
        private int vertexIndex;

        public EdgeChunk(GameObject _gameObject, Vector3[] _vertices, EdgeRenderer _edgeRenderer, int _subdivisions)
        {
            gameObject = _gameObject;

            // SETUP MESH
            edgeRenderer = _edgeRenderer;
            planetSettings = edgeRenderer.planetSettings;
            material = planetSettings.material;
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

            biome = NoiseFilterFactory.CreateNoiseFilter(planetSettings.biomeNoise);
            biomeBlendLine = NoiseFilterFactory.CreateNoiseFilter(planetSettings.biomeNoiseLine);

            // BIOMES
            if (planetSettings.biomes.Length == 0)
            {
                Debug.LogWarning("At least one biome must be defined.");
                return;
            }

            if (planetSettings.biomeStyle == BiomeStyle.Continent && planetSettings.biomes.Length < 3)
            {
                Debug.LogWarning("Continent biome style requires exactly 3 biomes.");
                return;
            }

            // terrainNoise[0] RED
            // terrainNoise[1] GREEN
            // terrainNoise[2] BLUE
            // terrainNoise[3] BLACK
            switch (planetSettings.biomeStyle)
            {
                case BiomeStyle.Frequency:
                    BiomesSameFrequency(planetSettings.biomes.Length);
                    break;
                case BiomeStyle.XY:
                    XYBiomes(planetSettings.biomes.Length, false);
                    break;
                case BiomeStyle.Continent:
                    BiomesContinentStyleFade();
                    break;
            }

            // Debug
            /*Debug.DrawLine(vertices[0], vertices[1], Color.red, 10000);
            Debug.DrawLine(vertices[1], vertices[2], Color.green, 10000);
            Debug.DrawLine(vertices[2], vertices[0], Color.blue, 10000);*/
        }

        /*
         * Same frequency.
         */
        private void BiomesSameFrequency(int numBiomes)
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
                    totalNoise = edgeRenderer.terrainNoise[0].Evaluate(vertices[i]);
                }

                if (numBiomes >= 2)
                {
                    if (biomeNoise < 0.6f + blendLineNoise)
                    {
                        var t = Utils.Utils.Remap(biomeNoise, 0.6f + blendLineNoise, 0.5f + blendLineNoise, 0, 1);
                        colors[i] = Color.Lerp(Color.red, Color.green, t);
                        totalNoise = Mathf.Lerp(edgeRenderer.terrainNoise[0].Evaluate(vertices[i]), edgeRenderer.terrainNoise[1].Evaluate(vertices[i]), t);
                    }

                    if (biomeNoise < 0.5f + blendLineNoise)
                    {
                        colors[i] = Color.green;
                        totalNoise = edgeRenderer.terrainNoise[1].Evaluate(vertices[i]);
                    }
                }

                if (numBiomes >= 3)
                {
                    if (biomeNoise < 0.4f + blendLineNoise)
                    {
                        var t = Utils.Utils.Remap(biomeNoise, 0.4f + blendLineNoise, 0.3f + blendLineNoise, 0, 1);
                        colors[i] = Color.Lerp(Color.green, Color.blue, t);
                        totalNoise = Mathf.Lerp(edgeRenderer.terrainNoise[1].Evaluate(vertices[i]), edgeRenderer.terrainNoise[2].Evaluate(vertices[i]), t);
                    }

                    if (biomeNoise < 0.3f + blendLineNoise)
                    {
                        colors[i] = Color.blue;
                        totalNoise = edgeRenderer.terrainNoise[2].Evaluate(vertices[i]);
                    }
                }

                if (numBiomes >= 4)
                {
                    if (biomeNoise < 0.2f + blendLineNoise)
                    {
                        var t = Utils.Utils.Remap(biomeNoise, 0.2f + blendLineNoise, 0.1f + blendLineNoise, 0, 1);
                        colors[i] = Color.Lerp(Color.blue, Color.black, t);
                        totalNoise = Mathf.Lerp(edgeRenderer.terrainNoise[2].Evaluate(vertices[i]), edgeRenderer.terrainNoise[3].Evaluate(vertices[i]), t);
                    }

                    if (biomeNoise < 0.1f + blendLineNoise)
                    {
                        colors[i] = Color.black;
                        totalNoise = edgeRenderer.terrainNoise[3].Evaluate(vertices[i]);
                    }
                }

                vertices[i] = vertices[i].normalized * (planetSettings.radius + totalNoise);
            }
        }

        /*
         * XY Biomes.
         */
        private void XYBiomes(int numBiomes, bool vert)
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
                totalNoise = edgeRenderer.terrainNoise[biomeIndex].Evaluate(vertices[i]);

                var biome = height * numBiomes;

                // Blending Zones
                for (int n = 1; n < numBiomes; n++)
                {
                    if (biome > n - edgeRenderer.blendRange3 && biome < n + edgeRenderer.blendRange3)
                    {
                        var biomeLine = -1 + ((float)n / numBiomes) * 2;
                        var t = Utils.Utils.Remap(axis, biomeLine - edgeRenderer.blendRange3 / numBiomes, biomeLine + edgeRenderer.blendRange3 / numBiomes, 0, 1);
                        colors[i] = Color.Lerp(biomeColors[n - 1], biomeColors[n], t);
                        totalNoise = Mathf.Lerp(edgeRenderer.terrainNoise[n - 1].Evaluate(vertices[i]), edgeRenderer.terrainNoise[n].Evaluate(vertices[i]), t);
                    }
                }

                vertices[i] = vertices[i].normalized * (planetSettings.radius + totalNoise);
            }
        }

        /*
         * All red.
         */
        private void OneBiome()
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                colors[i] = Color.red;
                var totalNoise = edgeRenderer.terrainNoise[0].Evaluate(vertices[i]);
                vertices[i] = vertices[i].normalized * (planetSettings.radius + totalNoise);
            }
        }

        /*
         * 50% red near top, 50% green near bottom, surrounded by blue.
         */
        private void BiomesContinentStyleFade()
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
                    var percentBlendLine = Mathf.InverseLerp(edgeRenderer.blendRange3, edgeRenderer.blendRange4, vertices[i].y + blendLine);
                    var r = 1 - percentBlendLine;
                    var g = percentBlendLine;

                    var redGreen = new Color(r, g, 0, 0);
                    var redGreenNoise = Mathf.Lerp(edgeRenderer.terrainNoise[0].Evaluate(vertices[i]), edgeRenderer.terrainNoise[1].Evaluate(vertices[i]), percentBlendLine);

                    totalNoise = Mathf.Lerp(edgeRenderer.terrainNoise[2].Evaluate(vertices[i]), redGreenNoise, percentBlend);
                    colors[i] = Color.Lerp(Color.blue, redGreen, percentBlend);
                }
                else
                {
                    totalNoise = edgeRenderer.terrainNoise[2].Evaluate(vertices[i]);
                    colors[i] = Color.blue;
                }

                vertices[i] = vertices[i].normalized * (planetSettings.radius + totalNoise);
            }
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