using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ColorManager))]
public class ColorManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ColorManager colorManager = (ColorManager)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Colorize"))
        {
            colorManager.AssignColors();
        }
    }
}
