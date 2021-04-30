using UnityEngine;

namespace SpaceGame
{
    public class PolySelector : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if (!Physics.Raycast(ray, out hit))
                    return;

                var meshCollider = hit.collider as MeshCollider;
                if (meshCollider == null || meshCollider.sharedMesh == null)
                    return;

                Mesh mesh = meshCollider.sharedMesh;

                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;

                Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
                Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
                Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

                Transform hitTransform = hit.collider.transform;

                p0 = hitTransform.TransformPoint(p0);
                p1 = hitTransform.TransformPoint(p1);
                p2 = hitTransform.TransformPoint(p2);

                Debug.DrawLine(p0, p1, Color.red, 0.5f);
                Debug.DrawLine(p1, p2, Color.green, 0.5f);
                Debug.DrawLine(p2, p0, Color.blue, 0.5f);

                // We populate the planet grid to let us know an entity exists in this space now
                //var planet = hit.transform.gameObject.GetComponent<Planet>();
                //planet.grid[hit.triangleIndex / 3] = 1;
            }
        }
    }
}