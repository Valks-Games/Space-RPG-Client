using UnityEngine;

// This script is attached to a directional light and acts as the "Sun"
// Perhaps this script should be merged into Game.cs Update()? (less Mono updates the better)
[ExecuteInEditMode]
public class Sun : MonoBehaviour
{
    public Transform cam; // The main camera

    private void Update()
    {
        transform.LookAt(cam); // Always look at the camera
    }
}