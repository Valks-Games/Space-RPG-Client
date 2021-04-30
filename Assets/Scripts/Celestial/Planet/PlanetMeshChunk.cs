using SpaceGame.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceGame.Celestial
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetMeshChunk : MonoBehaviour
    {
        private List<Vector3> vertices;
        private List<int> triangles;

        private Mesh mesh;

        public static int count = 0;

        public MeshRenderer meshRenderer;

        public void Create(PlanetMeshChunkRenderer _renderer, Vector3[] _vertices, int _chunkTriangles)
        {
            count++;
            gameObject.name = $"Chunk {count}";

            meshRenderer = GetComponent<MeshRenderer>();

            vertices = new List<Vector3> { _vertices[0], _vertices[1], _vertices[2] };
            triangles = new List<int>();

            var settings = _renderer.shapeSettings;

            if (_renderer.shapeType == PlanetMeshChunkRenderer.ShapeType.Noise)
                meshRenderer.material = settings.terrainMaterial;

            if (_renderer.shapeType == PlanetMeshChunkRenderer.ShapeType.Sphere)
                meshRenderer.material = settings.oceanMaterial;

            SubdivideFace(0, 1, 2, _chunkTriangles);

            var radius = settings.radius;

            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = vertices[i].normalized;

                switch (_renderer.shapeType)
                {
                    case PlanetMeshChunkRenderer.ShapeType.Noise:
                        float unscaledElevation = _renderer.shapeGenerator.CalculateUnscaledElevation(vertices[i]);
                        vertices[i] = vertices[i] * _renderer.shapeGenerator.GetScaledElevation(unscaledElevation);
                        break;

                    case PlanetMeshChunkRenderer.ShapeType.Sphere:
                        vertices[i] = vertices[i] * radius * (1 + settings.oceanDepth);
                        break;
                }
            }

            mesh = new Mesh();
            GetComponent<MeshFilter>().sharedMesh = mesh;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = vertices.Select(s => s.normalized).ToArray();

            _renderer.colourGenerator.GenerateColours(this);
        }

        /*!
         * The number of triangle recursions in this chunk.
         *
         * @param a - Top
         * @param b - Bottom Right
         * @param c - Bottom Left
         */

        private void SubdivideFace(int _top, int _bottomRight, int _bottomLeft, int n)
        {
            // No subdivisions
            if (n <= 1) 
            {
                triangles.AddRange(new List<int> { _top, _bottomRight, _bottomLeft});

                return;
            }

            vertices.Add(SphereUtils.GetMidPointVertex(vertices[_top], vertices[_bottomRight]));
            vertices.Add(SphereUtils.GetMidPointVertex(vertices[_bottomRight], vertices[_bottomLeft]));
            vertices.Add(SphereUtils.GetMidPointVertex(vertices[_bottomLeft], vertices[_top]));

            var middleRight = vertices.Count - 3;
            var middleBottom = vertices.Count - 2;
            var middleLeft = vertices.Count - 1;

            // Only draw the last recursion
            if (n == 2)
            {
                triangles.AddRange(new List<int> { _top, middleRight, middleLeft }); // Upper Top
                triangles.AddRange(new List<int> { middleLeft, middleBottom, _bottomLeft }); // Lower Left
                triangles.AddRange(new List<int> { middleBottom, middleLeft, middleRight }); // Lower Mid
                triangles.AddRange(new List<int> { middleRight, _bottomRight, middleBottom }); // Lower Right

                return;
            }

            SubdivideFace(_top, middleRight, middleLeft, n - 1);
            SubdivideFace(middleLeft, middleBottom, _bottomLeft, n - 1);
            SubdivideFace(middleBottom, middleLeft, middleRight, n - 1);
            SubdivideFace(middleRight, _bottomRight, middleBottom, n - 1);
        }

        public Vector3 GetCenterPoint() => SphereUtils.GetCenterPoint(vertices[0], vertices[1], vertices[2]);
    }
}