using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyShaderTest : MonoBehaviour
{
    public ComputeShader computeShader;
    public GameObject cubePrefab;

    public struct CubeData 
    {
        public Vector3 position;
        public Color color;
    }

    private CubeData[] cubeDataArray;
    private Transform[] cubes;

    private void Start()
    {
        cubeDataArray = new CubeData[10];
        cubes = new Transform[10];

        for (int x = 0; x < 10; x++) 
        {
            var cubeInstance = Instantiate(cubePrefab).transform;
            cubeInstance.position = new Vector3(x, 0, 0);

            CubeData cubeData = new CubeData();
            cubeData.position = cubeInstance.position;
            cubeData.color = cubeInstance.GetComponent<Renderer>().material.color;
            cubeDataArray[x] = cubeData;
            cubes[x] = cubeInstance;
        }

        int positionSize = sizeof(float) * 3;
        int colorSize = sizeof(float) * 4;
        int totalSize = positionSize + colorSize;
        ComputeBuffer cubesBuffer = new ComputeBuffer(cubeDataArray.Length, totalSize);
        cubesBuffer.SetData(cubeDataArray);
        computeShader.SetBuffer(0, "cubes", cubesBuffer);
        computeShader.SetFloat("resolution", cubeDataArray.Length);
        computeShader.Dispatch(0, cubeDataArray.Length / 10, 1, 1);

        cubesBuffer.GetData(cubeDataArray);

        for (int x = 0; x < 10; x++) 
        {
            cubes[x].GetComponent<Renderer>().material.color = cubeDataArray[x].color;
        }

        cubesBuffer.Dispose();
    }
}
