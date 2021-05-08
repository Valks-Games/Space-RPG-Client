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

            PlanetMeshChunk.count = 0; // To make the counting nicer in game object hiearchy
        }
    }
}