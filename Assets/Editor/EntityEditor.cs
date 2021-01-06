using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Entity))]
public class EntityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Entity entityMotor = (Entity)target;

        DrawDefaultInspector();

        GUILayout.Space(15);

        /*if (GUILayout.Button("Create marker"))
        {
            entityMotor.CreateMarker();
        }

        if (GUILayout.Button("Modify marker"))
        {
            entityMotor.ModifyMarker();
        }

        if (entityMotor.entityType != EntityType.None)
        {
            if (GUILayout.Button("Add essential components"))
            {
                entityMotor.AddEssentials();
            }
        }

        if(entityMotor.entityType == EntityType.None)
        {
            if(GUILayout.Button("Reset entity"))
            {
                entityMotor.ResetEntity();
            }
        }*/
    }
}
