using UnityEngine;

[ExecuteInEditMode]
public class Sun : MonoBehaviour
{
    public Transform cam; // camera

    private void Update()
    {
        transform.LookAt(cam);
    }
}