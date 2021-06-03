using SpaceGame.Celestial;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceGame
{
    public class EntitySelector : MonoBehaviour
    {
        /*private Dictionary<int, GameObject> selectedEntities = new Dictionary<int, GameObject>();

        private bool dragSelect;
        private Vector3 p1; // First mouse position on screen

        private Camera cam;
        private CameraController camScript;

        public GameObject planet;
        private Planet planetScript;

        private RaycastHit hit;
        private Vector2[] cornersScreenSpace;
        private Vector3[] cornersWorldSpace;

        // Debug
        public float debugDrawTime = 2f;

        public bool debugEnabled = false;

        private void Start()
        {
            cam = Camera.main;
            planetScript = planet.GetComponent<Planet>();
            camScript = cam.GetComponent<CameraController>();
        }

        private void Update()
        {
            #region Right Clicked

            if (Input.GetMouseButtonDown(1))
            {
                for (int i = 0; i < Game.groups.Count; i++)
                {
                    // Remove all idle groups
                    if (Game.groups[i].unitGroupTask == UnitGroupTask.Idle)
                    {
                        Game.groups.Remove(Game.groups[i]);
                    }
                }

                // Add Group from Selected
                if (selectedEntities.Count > 1)
                {
                    var units = new List<GameObject>();
                    foreach (var go in selectedEntities.Values)
                    {
                        go.GetComponent<Unit>().LeaveCurrentGroup(); // Remove old group if any
                        units.Add(go); // Add to new group
                    }

                    if (units.Count > 1)
                        Game.groups.Add(new UnitGroup(units, planetScript.transform));
                }

                // Since we were telling units to leave their old groups, we have to check if any groups have less than 2 members
                // Check if any groups have < 1 members
                for (int i = 0; i < Game.groups.Count; i++)
                {
                    Game.groups[i].RemoveGroupIfMemberCountLow();
                }

                // Give Movement Command
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, camScript.distanceFromPlanetSurface + planetScript.shapeSettings.radius, LayerMask.GetMask("Planets")))
                {
                    var selectedGroups = Game.groups.Where(group => group.IsSelected()).ToList();

                    if (selectedGroups.Count == 1)
                    {
                        selectedGroups[0].MoveToTarget(hit.point);
                    }
                    else if (selectedGroups.Count > 1)
                    {
                        var unitsForNewGroup = new List<GameObject>();

                        foreach (var group in selectedGroups)
                        {
                            // Selected more than 1 group
                            // Merge units selected from other groups into 1 new group
                            for (int i = 0; i < group.units.Count; i++)
                            {
                                var unit = group.units[i].GetComponent<Unit>();

                                if (!unit.selected)
                                    continue;

                                unit.LeaveCurrentGroup();
                                unitsForNewGroup.Add(group.units[i]);
                            }

                            group.RemoveGroupIfMemberCountLow();
                        }

                        var newGroup = new UnitGroup(unitsForNewGroup, planet.transform);
                        Game.groups.Add(newGroup);
                        newGroup.MoveToTarget(hit.point);
                    }
                }
            }

            #endregion Right Clicked

            #region Left Clicked

            if (Input.GetMouseButtonDown(0))
            {
                p1 = Input.mousePosition;
            }

            #endregion Left Clicked

            #region Hold Left Click

            if (Input.GetMouseButton(0))
            {
                if ((p1 - Input.mousePosition).magnitude > 20)
                    dragSelect = true;
            }

            #endregion Hold Left Click

            #region Left Click Released

            if (Input.GetMouseButtonUp(0))
            {
                if (dragSelect)
                    MarqueeSelect();
                else
                    SingleSelect();
            }

            #endregion Left Click Released
        }

        #region SelectionLogic

        private void MarqueeSelect()
        {
            cornersWorldSpace = new Vector3[4];
            cornersScreenSpace = GetBoundingBox(p1, Input.mousePosition);

            Vector3 camPos = cam.transform.position;

            for (int i = 0; i < cornersScreenSpace.Length; i++)
            {
                Ray ray = cam.ScreenPointToRay(cornersScreenSpace[i]);
                cornersWorldSpace[i] = ray.GetPoint(camScript.distanceFromPlanetSurface + planetScript.shapeSettings.radius);

                // DEBUG: Draw visual of 3D selection box.
                if (debugEnabled) Debug.DrawLine(camPos, cornersWorldSpace[i], Color.yellow, debugDrawTime);
            }

            if (!Input.GetKey(KeyCode.LeftShift))
            {
                // TODO: Would be more efficient to only deselect the ones that needed to be deselected based on the new selected
                DeslectAll();
            }

            foreach (var unit in Game.units)
            {
                var unitPos = unit.transform.position;

                // cornersWorldSpace[0] (top left)
                // cornersWorldSpace[1] (top right)
                // cornersWorldSpace[2] (bottom left)
                // cornersWorldSpace[3] (bottom right)

                // All planes are counter-clockwise
                var topPlane = new Plane(cornersWorldSpace[1], cornersWorldSpace[0], camPos);
                var leftPlane = new Plane(cornersWorldSpace[0], cornersWorldSpace[2], camPos);
                var rightPlane = new Plane(cornersWorldSpace[3], cornersWorldSpace[1], camPos);
                var bottomPlane = new Plane(cornersWorldSpace[2], cornersWorldSpace[3], camPos);

                //if (debugEnabled) Debug.Log(
                //    $"Top: {topPlane.GetSide(unitPos)}, " +
                //    $"Left: {leftPlane.GetSide(unitPos)}, " +
                //    $"Right: {rightPlane.GetSide(unitPos)}, " +
                //    $"Bottom: {bottomPlane.GetSide(unitPos)}");

                if (topPlane.GetSide(unitPos) && leftPlane.GetSide(unitPos) && rightPlane.GetSide(unitPos) && bottomPlane.GetSide(unitPos))
                {
                    if (Vector3.Distance(camPos, unitPos) < (planetScript.shapeSettings.radius + camScript.distanceFromPlanetSurface))
                    {
                        AddSelected(unit);
                    }
                }
            }

            dragSelect = false;
        }

        private void SingleSelect()
        {
            // Did p1 hit anything?
            Ray ray = cam.ScreenPointToRay(p1);
            if (Physics.Raycast(ray, out hit, 500000))
            {
                GameObject go = hit.transform.gameObject;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    // Shift add select
                    if (!IsSelected(go))
                    {
                        AddSelected(go);
                    }
                    else
                    {
                        Deselect(go);
                    }
                }
                else
                {
                    // Singular select
                    DeslectAll();
                    AddSelected(go);
                }
            }
            else // We did not hit anything.
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    // Do nothing
                }
                else
                {
                    DeslectAll();
                }
            }
        }

        #endregion SelectionLogic

        #region SelectionManagement

        private void AddSelected(GameObject go)
        {
            if (go.layer != LayerMask.NameToLayer("Units"))
                return;

            int id = go.GetInstanceID();

            if (!selectedEntities.ContainsKey(id))
            {
                selectedEntities.Add(id, go);

                go.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                go.GetComponent<Unit>().selected = true;
            }
        }

        private void Deselect(GameObject go)
        {
            selectedEntities[go.GetInstanceID()].GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            selectedEntities[go.GetInstanceID()].GetComponent<Unit>().selected = false;
            selectedEntities.Remove(go.GetInstanceID());
        }

        private void DeslectAll()
        {
            foreach (KeyValuePair<int, GameObject> pair in selectedEntities)
            {
                if (pair.Value != null)
                {
                    selectedEntities[pair.Key].GetComponent<Renderer>().material.color = Color.red;
                    selectedEntities[pair.Key].GetComponent<Unit>().selected = false;
                }
            }
            selectedEntities.Clear();
        }

        private bool IsSelected(GameObject go)
        {
            return selectedEntities.ContainsKey(go.GetInstanceID());
        }

        #endregion SelectionManagement

        #region Draw Screen Rect

        private void OnGUI()
        {
            if (dragSelect)
            {
                var rect = GetScreenRect(p1, Input.mousePosition);
                DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.8f, 0.2f));
                DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.8f));
            }
        }

        private void DrawScreenRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, WhiteTexture);
            GUI.color = Color.white;
        }

        private void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            // Top
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            // Left
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            // Right
            DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
            // Bottom
            DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        }

        //I do not fully understand this function copied from Unity RTS Selection Tutorial https://www.youtube.com/watch?v=OL1QgwaDsqo
        private Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
        {
            // Move origin from bottom left to top left
            screenPosition1.y = Screen.height - screenPosition1.y;
            screenPosition2.y = Screen.height - screenPosition2.y;
            // Calculate cornersScreenSpace
            var topLeft = Vector3.Min(screenPosition1, screenPosition2);
            var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
            // Create Rect
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        //Create a bounding box (4 cornersScreenSpace in order) from the start and end mouse position
        //I do not fully understand this function copied from Unity RTS Selection Tutorial https://www.youtube.com/watch?v=OL1QgwaDsqo

        private Vector2[] GetBoundingBox(Vector2 p1, Vector2 p2)
        {
            Vector2 newP1;
            Vector2 newP2;
            Vector2 newP3;
            Vector2 newP4;

            if (p1.x < p2.x) //if p1 is to the left of p2
            {
                if (p1.y > p2.y) // if p1 is above p2
                {
                    newP1 = p1;
                    newP2 = new Vector2(p2.x, p1.y);
                    newP3 = new Vector2(p1.x, p2.y);
                    newP4 = p2;
                }
                else //if p1 is below p2
                {
                    newP1 = new Vector2(p1.x, p2.y);
                    newP2 = p2;
                    newP3 = p1;
                    newP4 = new Vector2(p2.x, p1.y);
                }
            }
            else //if p1 is to the right of p2
            {
                if (p1.y > p2.y) // if p1 is above p2
                {
                    newP1 = new Vector2(p2.x, p1.y);
                    newP2 = p1;
                    newP3 = p2;
                    newP4 = new Vector2(p1.x, p2.y);
                }
                else //if p1 is below p2
                {
                    newP1 = p2;
                    newP2 = new Vector2(p1.x, p2.y);
                    newP3 = new Vector2(p2.x, p1.y);
                    newP4 = p1;
                }
            }

            Vector2[] cornersScreenSpace = { newP1, newP2, newP3, newP4 };
            return cornersScreenSpace;
        }

        private Texture2D _whiteTexture;

        private Texture2D WhiteTexture
        {
            get
            {
                if (_whiteTexture == null)
                {
                    _whiteTexture = new Texture2D(1, 1);
                    _whiteTexture.SetPixel(0, 0, Color.white);
                    _whiteTexture.Apply();
                }

                return _whiteTexture;
            }
        }

        #endregion Draw Screen Rect*/
    }
}