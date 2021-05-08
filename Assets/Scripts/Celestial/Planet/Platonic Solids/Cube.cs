using UnityEngine;

public class Cube
{
    private Vector3[] vertices;
    private int[] triangles;

    public Cube(float r) 
    {
        vertices = new Vector3[] { 
            new Vector3(-r, -r, -r),
            new Vector3(-r, -r, r),
            new Vector3(r, -r, r)
            //new Vector3(r, -r, -r)
        };

        triangles = new int[] { 
            2, 1, 0 
        };
    }

    public Vector3[] GetVertices() => vertices;

    public int[] GetTriangles() => triangles;

    public int GetFaceCount() => triangles.Length / 3;
}
