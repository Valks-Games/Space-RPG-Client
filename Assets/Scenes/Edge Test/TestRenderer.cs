using SpaceGame.Utils;
using System.Collections.Generic;
using UnityEngine;

public class TestRenderer : MonoBehaviour
{
    public Material mat;
    public int chunkSubdivisions;

    private void Start()
    {
        chunkSubdivisions = Mathf.Max(chunkSubdivisions, 0);
        var directions = new Vector3[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < directions.Length; i++)
            new TestTerrainFace(directions[i], chunkSubdivisions, mat);
    }
}