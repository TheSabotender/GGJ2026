using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(Localized))]
public class LocalizedEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var localizationKeyProperty = serializedObject.FindProperty("localizationKey");

        var localizationKeyField = new ToolbarPopupSearchField
        {
            label = localizationKeyProperty.displayName,
            value = localizationKeyProperty.stringValue
        };

        localizationKeyField.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue == localizationKeyProperty.stringValue)
            {
                return;
            }

            serializedObject.Update();
            localizationKeyProperty.stringValue = evt.newValue;
            serializedObject.ApplyModifiedProperties();

            foreach (var targetObject in targets)
            {
                if (targetObject is Localized localized)
                {
                    localized.RefreshLocalization();
                }
            }
        });

        root.Add(localizationKeyField);

        var textComponentProperty = serializedObject.FindProperty("textComponent");
        if (textComponentProperty != null)
        {
            var textComponentField = new PropertyField(textComponentProperty);
            textComponentField.SetEnabled(false);
            root.Add(textComponentField);
        }

        return root;
    }
}
