using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Icosahedron
{
    private Vector3[] vertices;

    public Icosahedron(float radius = 1) 
    {
        var t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        vertices = new Vector3[]
        {
            new Vector3(-1, t, 0).normalized  * radius,
            new Vector3(1, t, 0).normalized   * radius,
            new Vector3(-1, -t, 0).normalized * radius,
            new Vector3(1, -t, 0).normalized  * radius,
            new Vector3(0, -1, t).normalized  * radius,
            new Vector3(0, 1, t).normalized   * radius,
            new Vector3(0, -1, -t).normalized * radius,
            new Vector3(0, 1, -t).normalized  * radius,
            new Vector3(t, 0, -1).normalized  * radius,
            new Vector3(t, 0, 1).normalized   * radius,
            new Vector3(-t, 0, -1).normalized * radius,
            new Vector3(-t, 0, 1).normalized  * radius
        };
    }

    public Vector3[] GetVertices() => vertices;
}
