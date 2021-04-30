using SpaceGame.Celestial;
using UnityEngine;

namespace SpaceGame
{
    public class CameraController : MonoBehaviour
    {
        [Header("Planet")]
        public Transform planet;

        [Header("Speed")]
        [Range(0.0f, 1.0f)]
        public float rotationSpeed = 1f;

        [Range(0.0f, .5f)]
        public float scrollFactor = .1f;

        [Range(0.0f, 1.0f)]
        public float scrollSpeed = .1f;

        private Planet planetScript;
        private Vector3 previousPosition;
        private Camera cam;

        public float distanceFromPlanetSurface = 100;
        private float targetDistanceFromPlanet;

        private void Awake()
        {
            cam = gameObject.AddComponent<Camera>();
            gameObject.tag = "MainCamera"; // This is the main camera
            gameObject.name = "Main Camera";

            targetDistanceFromPlanet = distanceFromPlanetSurface;
        }

        // Late update because then we know the positions of all the objects moving in update.
        private void LateUpdate()
        {
            // Mouse button codes
            // 0 = primary
            // 1 = secondary
            // 2 = middle

            // Handle zoom with scrolling in and out.
            // Zoom speed is handled based on how close you are to the planet.
            targetDistanceFromPlanet *= 1 - scrollFactor * Input.mouseScrollDelta.y;

            distanceFromPlanetSurface += (targetDistanceFromPlanet - distanceFromPlanetSurface) * scrollSpeed;

            // Handle rotation around planet with middle mouse button drag
            if (Input.GetMouseButton(2))
            {
                Vector3 dir = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);

                // Multiplying by the distance from the planet allows for more pracise rotation when closer to the planet
                cam.transform.Rotate(new Vector3(1, 0, 0), (dir.y * 180) * rotationSpeed * (distanceFromPlanetSurface / 100));
                cam.transform.Rotate(new Vector3(0, 1, 0), -(dir.x * 180) * rotationSpeed * (distanceFromPlanetSurface / 100));
            }
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);

            // Recalculate position
            //cam.transform.position = planet.position;
            //cam.transform.Translate(new Vector3(0, 0, -planetScript.radius - distanceFromPlanetSurface));

            //cam.farClipPlane = distanceFromPlanetSurface + planetScript.radius;
        }

        public void FocusOnPlanet(GameObject _planetGo)
        {
            planet = _planetGo.transform;
            planetScript = planet.GetComponent<Planet>();
            cam.transform.Translate(new Vector3(0, 0, -planetScript.shapeSettings.radius - distanceFromPlanetSurface));
        }
    }
}