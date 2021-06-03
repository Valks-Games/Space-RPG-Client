using SpaceGame.Celestial;
using SpaceGame.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceGame
{
    public enum UnitGroupTask
    {
        Idle,
        MoveToTarget
    }

    public class UnitGroup
    {
        public List<GameObject> units;
        public Transform planet;
        private float planetRadius;

        private static int groupCount = 0;
        private Transform groupOrigin;

        public UnitGroupTask unitGroupTask;
        public Vector3 target;

        private float distanceBetweenAgents;

        public UnitGroup(List<GameObject> units, Transform planet)
        {
            this.units = units;
            this.planet = planet;
            //planetRadius = planet.GetComponent<Planet>().shapeSettings.radius;

            // Initialize group origin, all units will align with respect to this origin
            groupOrigin = new GameObject().transform;
            groupOrigin.transform.position = units[0].transform.position;
            groupCount++;
            groupOrigin.name = $"({groupCount}) Group";

            // Assign all units to group
            foreach (var unit in units)
                unit.GetComponent<Unit>().group = this;

            unitGroupTask = UnitGroupTask.Idle;

            distanceBetweenAgents = 1f;
        }

        public void Update()
        {
            if (unitGroupTask == UnitGroupTask.MoveToTarget)
            {
                SphereUtils.AlignToSphereSurface(groupOrigin, planet);
                SphereUtils.LookAtTarget(groupOrigin, planet, target);

                AlignWithGroupOrigin();

                var speed = 10f;
                groupOrigin.position = Vector3.RotateTowards(groupOrigin.position, target, (speed / planetRadius) * Time.deltaTime, 1);

                if (Vector3.Distance(groupOrigin.position, target) < 1)
                {
                    // Additional logic needed eventually.
                    unitGroupTask = UnitGroupTask.Idle;
                }
            }
        }

        public void RemoveUnit(Unit unit)
        {
            if (!units.Remove(unit.gameObject)) // Remove unit from this group
                return;

            if (RemoveGroupIfMemberCountLow()) // Remove this group if member count is too low
                return;

            unit.Idle(); // Make sure the unit stops trying to move to their target
            unit.group = null;
        }

        public bool IsSelected() =>
            units.Any(unit => unit.GetComponent<Unit>().selected);

        public bool RemoveGroupIfMemberCountLow()
        {
            if (GetMemberCount() > 1)
                return false;

            //groupCount--;
            Object.Destroy(groupOrigin.gameObject);
            Game.groups.Remove(this);
            return true;
        }

        public void MoveToTarget(Vector3 target)
        {
            this.target = target;
            unitGroupTask = UnitGroupTask.MoveToTarget;
        }

        public void AlignWithGroupOrigin()
        {
            Debug.DrawRay(groupOrigin.position, groupOrigin.forward * 10f, Color.green);

            SquareFormationV1();
        }

        /*
         * Seems to work for all cases but movement is sort of weird. It is clear that unit formations
         * is much bigger then I anticipated and will require alot more thinking.
         */

        private void SquareFormation()
        {
            var unitsInRow = Mathf.CeilToInt(Mathf.Sqrt(this.units.Count));
            var rows = Mathf.CeilToInt((float)this.units.Count / unitsInRow);

            var back = -this.groupOrigin.forward;
            var right = this.groupOrigin.right;

            for (int i = 0; i < rows; i++)
            {
                var isLastRow = i + 1 == rows;
                var countInRow = isLastRow ? this.units.Count - unitsInRow * (rows - 1) : unitsInRow;
                var offset = -right * ((countInRow - 1f) / 2f);

                for (int j = 0; j < countInRow; j++)
                {
                    var unitIndex = i * unitsInRow + j;

                    var pos = groupOrigin.position + i * back + j * right + offset;

                    // Snap back to planets surface
                    var newGravityUp = (pos - planet.position).normalized * (planetRadius + 1);
                    pos = newGravityUp;

                    // Slowly move towards these positions
                    units[unitIndex].GetComponent<Unit>().MoveToTarget(pos);
                }
            }
        }

        public int GetMemberCount() => units.Count;

        /*
         * Square Formation (does not work for odd row numbers)
         */

        private void SquareFormationV1()
        {
            var horzDist = 0f;
            var vertDist = 0f;

            for (int i = 0; i < units.Count; i++)
            {
                // Swap between left and right directions
                Vector3 vertDir = groupOrigin.forward * -1;
                Vector3 horzDir;
                if (i % 2 == 0)
                {
                    horzDir = groupOrigin.right * -1;
                }
                else
                {
                    horzDir = groupOrigin.right;
                    horzDist += distanceBetweenAgents;
                }

                // Start a new row behind
                if (i % 10 == 0 && i != 0)
                {
                    horzDist = 0f;
                    vertDist += distanceBetweenAgents;
                }

                // Calculate positions at end of each blue line
                var pos = groupOrigin.position + (horzDir * horzDist) + (vertDir * vertDist);

                // Snap back to planets surface
                var newGravityUp = (pos - planet.position).normalized * (planetRadius + 1);
                pos = newGravityUp;

                // Slowly move towards these positions
                units[i].GetComponent<Unit>().MoveToTarget(pos);
                //Debug.DrawLine(groupOrigin.position, pos, Color.blue);
            }
        }

        /*
         * Does not work because centerUnit rotation is not same as original leader rotation.
         */

        private void FollowCenterUnit(Transform centerUnit, int n = 0, int horzDir = 1, int rows = 0)
        {
            if (n % 3 == 0 && n != 0)
            {
                centerUnit = units[n - 3].transform;
                horzDir = -horzDir;
            }

            /*if (n % 6 == 0 && n != 0)
            {
                rows++;
            }*/

            // Position
            var horz = centerUnit.right * horzDir * distanceBetweenAgents;
            var vert = centerUnit.forward * -1 * rows * distanceBetweenAgents;
            var pos = centerUnit.position + horz + vert;

            // Snap back to planets surface
            var newGravityUp = (pos - planet.position).normalized * (planetRadius + 1);
            pos = newGravityUp;

            if (n + 1 >= units.Count)
                return;

            units[n + 1].GetComponent<Unit>().MoveToTarget(pos);

            FollowCenterUnit(units[n + 1].transform, n + 1, horzDir, rows);
        }

        /*
         * I honestly don't get it at all......
         */
        /*private void SquareFormationV2()
        {
            var dirRight = groupOrigin.right;
            var dirForward = groupOrigin.forward;
            var startPos = groupOrigin.position;
            var x = -(units.Count / 10) / 2 - 1;
            var z = -(units.Count / 10) / 2;

            for (int i = 0; i < units.Count; i++)
            {
                x++;
                if (x % 10 == 0)
                {
                    x = 0;
                    z++;
                }

                var curPos = startPos + dirRight * x + dirForward * z;

                units[i].GetComponent<Unit>().MoveToTarget(curPos);
            }
        }*/

        /*
         * Just ignore this.
         */
        /*private void FormationTestCosSin()
        {
            for (int i = 0; i < units.Count; i++)
            {
                var posX = planet.position.x + Mathf.Cos(5 * i) * planetRadius;
                var posY = planet.position.y + Mathf.Sin(0 * i) * planetRadius;
                var pos = new Vector3(posX, posY, 0);

                //var newGravityUp = (pos - planet.position).normalized * (planetRadius + 1);
                //pos = newGravityUp;

                // Slowly move towards these positions
                //units[i].GetComponent<Unit>().MoveToTarget(pos);
            }
        }*/

        /*
         * Does not work because 1) can't pass the north / south pole (units have to go all the way around)
         * and 2) the spherical coordinate system is a uneven 2D grid map distribution resulting in the units
         * getting scrunched near the poles of the planet.
         */
        /*private void SphericalCoordFormationTest()
        {
            var prevSphereGroupOriginPoint = PlanetUtils.CartesianToSpherical(groupOrigin.position);
            var curSphereGroupOrigin = prevSphereGroupOriginPoint;
            var vertDist = (50f / planetRadius);
            var horzDist = (50f / planetRadius);

            for (int i = 0; i < units.Count; i++)
            {
                curSphereGroupOrigin.z += (vertDist * Mathf.Deg2Rad);

                if (i % 10 == 0)
                {
                    curSphereGroupOrigin.z = prevSphereGroupOriginPoint.z;
                    curSphereGroupOrigin.y += (horzDist * Mathf.Deg2Rad);
                }

                var pos = PlanetUtils.SphericalToCartesian(curSphereGroupOrigin);

                // Snap back to planets surface
                var newGravityUp = (pos - planet.position).normalized * (planetRadius + 1);
                pos = newGravityUp;

                units[i].GetComponent<Unit>().MoveToTarget(pos);
            }
        }*/
    }
}