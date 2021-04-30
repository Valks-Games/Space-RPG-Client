using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SpaceGame.Celestial;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    private Planet planet;

    private Editor planetEditor;
    private Editor terrainShapeEditor;
    private Editor colourEditor;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                planet.GeneratePlanet();
            }
        }
        CreateButton("Generate Planet", planet.GeneratePlanet);

        DrawSettingsEditor(planet.planetSettings, () => planet.OnPlanetSettingsUpdated(false), ref planet.planetSettingsFoldout, ref planetEditor);
        CreateButton("Update Planet", () => planet.OnPlanetSettingsUpdated(true));

        DrawSettingsEditor(planet.shapeSettings, () => planet.OnShapeSettingsUpdated(false), ref planet.shapeSettingsFoldout, ref terrainShapeEditor);
        CreateButton("Update Shape", () => planet.OnShapeSettingsUpdated(true));

        DrawSettingsEditor(planet.colourSettings, () => planet.OnColourSettingsUpdated(false), ref planet.colourSettingsFoldout, ref colourEditor);
        CreateButton("Update Colours", () => planet.OnColourSettingsUpdated(true));
    }

    private void CreateButton(string name, System.Action onButtonPressed) 
    {
        GUILayout.Space(10);
        if (GUILayout.Button(name))
        {
            onButtonPressed();
        }
        GUILayout.Space(10);
    }

    private void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        if (onSettingsUpdated != null)
                        {
                            onSettingsUpdated();
                        }
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        planet = (Planet)target;
    }
}