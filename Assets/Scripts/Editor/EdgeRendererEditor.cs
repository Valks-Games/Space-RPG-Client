using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SpaceGame.Celestial;

[CustomEditor(typeof(EdgeRenderer))]
public class EdgeRendererEditor : Editor
{
    private EdgeRenderer edgeRenderer;

    private Editor biomeEditor;
    //private Editor terrainShapeEditor;
    //private Editor colourEditor;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                edgeRenderer.GeneratePlanet();
                //edgeRenderer.UpdateUVs();
            }
        }
        CreateButton("Generate Planet", edgeRenderer.GeneratePlanet);
        CreateButton("Update UVs", edgeRenderer.UpdateUVs);

        //DrawSettingsEditor(edgeRenderer.biomeSettings, () => edgeRenderer.OnBiomeSettingsUpdated(false), ref edgeRenderer.biomeSettingsFoldout, ref biomeEditor);
        //CreateButton("Update Planet", () => planet.OnPlanetSettingsUpdated(true));
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
        edgeRenderer = (EdgeRenderer)target;
    }
}