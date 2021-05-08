using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SpaceGame.Utils;

// reminder to add namespace when converting over
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TestChunk : MonoBehaviour
{
    private Vector3[] vertices;
    private Edge[] edges;
    private int[] triangles;
    private Vector2[] uvs;

    private int vertexIndex;
    private int triIndex;

    private Material mat;
    private TestTerrainFace face;

    public void Create(TestTerrainFace _face, Vector2[] _points, Material _mat, int _subdivisions = 0)
    {
        face = _face;

        var vertexCount = (int)Mathf.Pow(_subdivisions + 2, 2);
        vertices = new Vector3[vertexCount];

        var triangleCount = vertexCount * 3;
        triangles = new int[triangleCount];

        mat = _mat;

        var numInnerEdgeVertices = 2;

        edges = new Edge[4];
        edges[0] = new Edge(_points[0], _points[1], numInnerEdgeVertices); // Top
        edges[1] = new Edge(_points[1], _points[3], numInnerEdgeVertices); // Right
        edges[2] = new Edge(_points[3], _points[2], numInnerEdgeVertices); // Bottom
        edges[3] = new Edge(_points[2], _points[0], numInnerEdgeVertices); // Left

        for (int i = 0; i < vertices.Length; i++) 
        {
            
        }

        for (int i = 0; i < edges.Length; i++)
            for (int j = 0; j < edges[i].vertices.Length; j++)
                new DebugPoint((face.localUp + (edges[i].vertices[j].x - 0.5f) * 2 * face.axisA + (edges[i].vertices[j].y - 0.5f) * 2 * face.axisB).normalized).SetColor(Color.red);

        GenerateMesh();
    }

    public void GenerateMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray(); ;
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;
        mesh.normals = mesh.vertices.Select(s => s.normalized).ToArray();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().material = mat;
    }

    public void SetColor(Color color)
    {
        GetComponent<MeshRenderer>().material.SetColor("_Color", color);
    }
}

public class Edge 
{
    public Vector3[] vertices;

    public Edge(Vector3 start, Vector3 end, int numInnerVertices) 
    {
        vertices = new Vector3[numInnerVertices + 1];

        for (int i = 0; i <= numInnerVertices; i++) 
        {
            var t = (float)i / numInnerVertices;
            vertices[i] = Vector3.Lerp(start, end, t);
        }
    }
}