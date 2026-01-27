using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterProfile))]
public class CharacterProfileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        EnsureGuids();

        EditorGUILayout.Space();
        if (GUILayout.Button("New Guid"))
        {
            AssignNewGuids("Assign New Guid");
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void EnsureGuids()
    {
        foreach (var targetObject in targets)
        {
            var profile = (CharacterProfile)targetObject;
            if (string.IsNullOrWhiteSpace(profile.Guid))
            {
                AssignGuid(profile, "Auto Assign Guid");
            }
        }
    }

    private void AssignNewGuids(string actionName)
    {
        foreach (var targetObject in targets)
        {
            var profile = (CharacterProfile)targetObject;
            AssignGuid(profile, actionName);
        }
    }

    private static void AssignGuid(CharacterProfile profile, string actionName)
    {
        Undo.RecordObject(profile, actionName);
        profile.NewGuid();
        EditorUtility.SetDirty(profile);
    }
}
