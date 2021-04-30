using SpaceGame.Celestial;
using SpaceGame.Utils;
using UnityEngine;

namespace SpaceGame
{
    public enum UnitTask
    {
        Idle,
        MoveToTarget
    }

    // The player is a temporary class to help with debugging.
    public class Unit : MonoBehaviour
    {
        public Transform planet;
        private Planet planetScript;

        private float planetRadius;

        private static int unitCount = 0;

        private Vector3 gravityUp;

        public UnitTask unitTask;

        private float speed = 10f;

        public UnitGroup group; // The group this unit belongs to

        public bool selected;

        private Vector3 target;

        private int unitsLayerMask;

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Units");
        }

        private void Start()
        {
            planetScript = planet.GetComponent<Planet>();
            planetRadius = planetScript.shapeSettings.radius;

            transform.position = new Vector3(0, planetRadius + 1, 0);

            gameObject.name = $"({++unitCount}) Unit";

            AddRandomForce(0.00001f);

            unitTask = UnitTask.Idle;

            unitsLayerMask = LayerMask.GetMask("Units");
        }

        private void Update()
        {
            //Separation();
            SphereUtils.AlignToSphereSurface(transform, planet);
            SphereUtils.LookAtTarget(transform, planet, target);

            if (unitTask == UnitTask.MoveToTarget)
            {
                MovementLogic();
            }
        }

        public void MoveToTarget(Vector3 target)
        {
            unitTask = UnitTask.MoveToTarget;
            this.target = target;
        }

        private void MovementLogic()
        {
            // Move towards the target
            var distanceToTarget = Vector3.Distance(transform.position, target);
            if (distanceToTarget > 0)
            {
                // Moving towards target
                transform.position = Vector3.RotateTowards(transform.position, target, (speed / planetRadius) * Time.deltaTime, 1);
            }
        }

        /*
        * Separate agents from each other.
        */

        public void Separation()
        {
            gravityUp = (transform.position - planet.position).normalized;

            int maxColliders = Game.units.Count;
            Collider[] hitColliders = new Collider[maxColliders];
            var numColliders = Physics.OverlapSphereNonAlloc(transform.position, 1.1f, hitColliders, unitsLayerMask);

            for (int i = 0; i < numColliders; i++)
            {
                var agentA = transform.position;
                var agentB = hitColliders[i].transform.position;

                var unit = hitColliders[i].GetComponent<Unit>();

                var maxDist = 1.1f; // Default separation force
                                    //if (unit.group != null)
                                    //maxDist = unit.group.distanceBetweenAgents; // Adjust to groups distance between agents

                var curDist = (agentB - agentA).sqrMagnitude;

                if (curDist < maxDist)
                {
                    var dir = (agentA - agentB).normalized;

                    // Separate the agents from each other
                    var separationForce = maxDist - curDist;
                    transform.position += dir * separationForce * Time.deltaTime;
                }
            }
        }

        /*
         * Add a random directional force to the unit.
         * Mainly used for separating units on top of each other.
         * A force of 0.00001f is the absolute minimum required to make any difference.
         */

        private void AddRandomForce(float force)
        {
            // Add random force so they separate if spawned on top of each other
            float separationAngle = Random.Range(0, 2 * Mathf.PI);
            Vector3 separationDirection = new Vector3(Mathf.Cos(separationAngle), 0, Mathf.Sin(separationAngle));
            transform.position += separationDirection * force;
        }

        /*
         * Leave current group if any.
         */

        public void LeaveCurrentGroup() =>
            group?.RemoveUnit(this);

        public void Idle() =>
            unitTask = UnitTask.Idle;

        public void SetMaxSpeed(float value) =>
            speed = value;
    }
}