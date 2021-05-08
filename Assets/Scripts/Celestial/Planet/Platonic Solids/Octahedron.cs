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

    // Does not work if the triangles are their own separate meshes.
    public static Vector2[] CalculateUVs(Vector3[] vertices)
    {
        var uv = new Vector2[vertices.Length];

        float previousX = 1f;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];

            if (v.x == previousX)
            {
                uv[i - 1].x = 1f;
            }

            previousX = v.x;
            Vector2 textureCoordinates;
            textureCoordinates.x = Mathf.Atan2(v.x, v.z) / (-2f * Mathf.PI);

            if (textureCoordinates.x < 0f)
            {
                textureCoordinates.x += 1f;
            }

            textureCoordinates.y = Mathf.Asin(v.y) / Mathf.PI + 0.5f;
            uv[i] = textureCoordinates;
        }

        uv[vertices.Length - 4].x = uv[0].x = 0.125f;
        uv[vertices.Length - 3].x = uv[1].x = 0.375f;
        uv[vertices.Length - 2].x = uv[2].x = 0.625f;
        uv[vertices.Length - 1].x = uv[3].x = 0.875f;

        return uv;
    }

    public Vector3[] GetVertices() => vertices;

    public int[] GetTriangles() => triangles;

    public int GetFaceCount() => triangles.Length / 3;
}
