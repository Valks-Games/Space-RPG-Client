using UnityEngine;

public class Octahedron
{
    private Vector3[] vertices;
    private int[] triangles;

    public Octahedron(float radius = 1)
    {
        vertices = new Vector3[] {
            Vector3.down, Vector3.down, Vector3.down, Vector3.down,
            Vector3.forward,
            Vector3.left,
            Vector3.back,
            Vector3.right,
            Vector3.forward,
            Vector3.up, Vector3.up, Vector3.up, Vector3.up
        };

        triangles = new int[] {
            0, 4, 5,
            1, 5, 6,
            2, 6, 7,
            3, 7, 8,

             9, 5, 4,
            10, 6, 5,
            11, 7, 6,
            12, 8, 7
        };
    }

    public Vector3[] GetVertices() => vertices;
    public int[] GetTriangles() => triangles;
}
