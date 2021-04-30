using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public class Game : MonoBehaviour
    {
        public static List<GameObject> units = new List<GameObject>();
        public static List<UnitGroup> groups = new List<UnitGroup>();

        public int planetRadius = 19;
        private Transform planet;

        private void Awake()
        {
            // Create main camera
            var cameraGo = new GameObject();
            cameraGo.transform.Rotate(new Vector3(90, 0, 0));
            var camera = cameraGo.AddComponent<CameraController>();

            // Create planet
            /*var planetGo = new GameObject();
            var planet = planetGo.AddComponent<Planet>();

            var planetSettings = new PlanetSettings();
            planetSettings.name = "Yomolla";
            planet.planetSettings = planetSettings;

            var shapeSettings = new ShapeSettings();
            shapeSettings.radius = planetRadius;
            shapeSettings.generateNoise = true;
            shapeSettings.renderRadius = 1000;
            shapeSettings.material = Resources.Load<Material>("Materials/Planet");
            planet.shapeSettings = shapeSettings;

            var waterShapeSettings = new ShapeSettings();
            waterShapeSettings.radius = planetRadius + 2;
            waterShapeSettings.generateNoise = false;
            waterShapeSettings.renderRadius = 1000;
            waterShapeSettings.material = Resources.Load<Material>("Materials/Water");
            planet.waterShapeSettings = waterShapeSettings;

            planet.Create();

            this.planet = planetGo.transform;

            camera.FocusOnPlanet(planetGo);*/

            /*
            // Create units
            var unitGoPrefab = Resources.Load<GameObject>("Prefabs/Unit");
            for (int i = 0; i < 100; i++)
            {
                var unitGo = Instantiate(unitGoPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                var unit = unitGo.GetComponent<Unit>();

                unit.planet = planetGo.transform;
                unitGo.transform.position = unit.planet.position + new Vector3(0, planet.radius + 1, 0);

                units.Add(unitGo);
            }

            // Create entity selector
            var entitySelector = gameObject.AddComponent<EntitySelector>();
            entitySelector.planet = planetGo;*/
        }

        private void Update()
        {
            foreach (var unit in units)
            {
                //Unit unitScript = unit.GetComponent<Unit>();
            }

            foreach (var group in groups)
            {
                group.Update();
            }
        }
    }
}