using SpaceGame.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceGame.Celestial
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetMeshChunk : MonoBehaviour
    {
        public List<Vector3> vertices = new List<Vector3>();
        private int[] triangles;

        private Mesh mesh;

        public static int count = 0;
        public int subdivisions;

        private int lastEdgeVertex;
        private int edgeIndex;
        private int rowIndex;
        private int triIndex;

        public MeshRenderer meshRenderer;
        private PlanetMeshChunkRenderer meshChunkrenderer;

        public Vector3 GetCenterPoint() => SphereUtils.GetCenterPoint(vertices[0], vertices[1], vertices[2]);
    }
}