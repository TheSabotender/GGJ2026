using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CharacterAnimOverrideCreator
{
    private const string MenuItemPath = "Assets/Add CharacterAnims";

    [MenuItem(MenuItemPath, true)]
    private static bool AddCharacterAnimsValidate()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path);
    }

    [MenuItem(MenuItemPath)]
    private static void AddCharacterAnims()
    {
        var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("Add CharacterAnims", "Please select a folder in the Project window.", "OK");
            return;
        }

        var baseController = FindBaseController();
        if (baseController == null)
        {
            EditorUtility.DisplayDialog("Add CharacterAnims", "Could not find an AnimatorController named 'BaseCharacter'.", "OK");
            return;
        }

        var overrideController = new AnimatorOverrideController(baseController);
        var overridePath = AssetDatabase.GenerateUniqueAssetPath(
            Path.Combine(folderPath, "CharacterAnims.overrideController"));
        AssetDatabase.CreateAsset(overrideController, overridePath);

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);
        for (var i = 0; i < overrides.Count; i++)
        {
            var originalClip = overrides[i].Key;
            if (originalClip == null)
            {
                continue;
            }

            var newClip = new AnimationClip
            {
                name = originalClip.name
            };
            var clipPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(folderPath, $"{originalClip.name}.anim"));
            AssetDatabase.CreateAsset(newClip, clipPath);
            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(originalClip, newClip);
        }

        overrideController.ApplyOverrides(overrides);
        EditorUtility.SetDirty(overrideController);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = overrideController;
    }

    private static AnimatorController FindBaseController()
    {
        var controllerGuids = AssetDatabase.FindAssets("BaseCharacter t:AnimatorController");
        if (controllerGuids.Length == 0)
        {
            return null;
        }

        var controllerPath = AssetDatabase.GUIDToAssetPath(controllerGuids[0]);
        return AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
    }
}
