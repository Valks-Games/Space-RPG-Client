using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceGame.Utils;

public class TestTerrainFace
{
    public Vector3 localUp;
    public Vector3 axisA;
    public Vector3 axisB;
    public Material mat;
    public int chunkSubdivisions;

    public TestTerrainFace(Vector3 _localUp, int _chunkSubdivisions, Material _mat)
    {
        mat = _mat;
        localUp = _localUp;
        chunkSubdivisions = _chunkSubdivisions;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);

        Debug.DrawRay(Vector3.zero, axisA, Color.red, Mathf.Infinity);
        Debug.DrawRay(Vector3.zero, axisB, Color.red, Mathf.Infinity);

        // Calculate points
        /*for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                var percent = new Vector2(x, y) / (resolution - 1);
                var pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                new DebugPoint(pointOnUnitCube).SetSize(1).SetColor(Color.red);
                vertices[vertexIndex++] = pointOnUnitCube;
            }
        }*/


        SubdividePlane(_chunkSubdivisions);
    }

    private void SubdividePlane(int n, float centerX = 0.5f, float centerY = 0.5f, float length = 0.5f) 
    {
        if (n == 0) 
        {
            /*TestPoint(centerX - length, centerY - length); // Top Left
            TestPoint(centerX + length, centerY - length); // Top Right
            TestPoint(centerX - length, centerY + length); // Bottom Left
            TestPoint(centerX + length, centerY + length); // Bottom Right*/

            Vector2[] points = new Vector2[4] {
                new Vector2(centerX - length, centerY - length),
                new Vector2(centerX + length, centerY - length),
                new Vector2(centerX - length, centerY + length),
                new Vector2(centerX + length, centerY + length)
            };

            new GameObject().AddComponent<TestChunk>().Create(this, points, mat, 0);

            return;
        }

        SubdividePlane(n - 1, centerX - length / 2, centerY - length / 2, length / 2); // Top Left
        SubdividePlane(n - 1, centerX + length / 2, centerY - length / 2, length / 2); // Top Right
        SubdividePlane(n - 1, centerX - length / 2, centerY + length / 2, length / 2); // Bottom Left
        SubdividePlane(n - 1, centerX + length / 2, centerY + length / 2, length / 2); // Bottom Right
    }

    public void TestPoint(float x, float y) 
    {
        var pointOnUnitCube = localUp + (x - 0.5f) * 2 * axisA + (y - 0.5f) * 2 * axisB;
        new DebugPoint(pointOnUnitCube, $"X: {x}, Y: {y}").SetSize(1).SetColor(Color.red);
    }
}
