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
            var chunks = shapeSettings.chunks; // number of chunk recursions per base face

            if (shapeType == ShapeType.Sphere)
                chunks = shapeSettings.oceanChunks;

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

            planet.transform.localPosition = planet.curLocalPos;
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
    }
}