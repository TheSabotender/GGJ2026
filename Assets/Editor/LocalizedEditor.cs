using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(Localized))]
public class LocalizedEditor : Editor
{
    public class Category
    {
        public string name;
        public List<string> entries;
    }

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var localizationKeyProperty = serializedObject.FindProperty("localizationKey");

        var localizationKeyField = new ToolbarPopupSearchField();

        localizationKeyField.value = localizationKeyProperty.stringValue;

        localizationKeyField.menu.AppendAction("None", action =>
        {
            localizationKeyField.value = string.Empty;
        }, DropdownMenuAction.AlwaysEnabled);

        foreach (var cat in SortKeys(LocalizationManager.AllKeys()))
        {
            localizationKeyField.menu.AppendSeparator();

            foreach (var entry in cat.entries)
            {
                localizationKeyField.menu.AppendAction(entry, action => {
                    localizationKeyField.value = cat.name + "-" + entry;
                });
            }            
        }        

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

    private List<Category> SortKeys(IEnumerable<string> keys)
    {
        var list = new List<Category>();

        foreach (var key in keys)
        {
            var category = key.Split('-')[0];
            var entry = key.Split("-")[1];

            var cat = list.FirstOrDefault(c => c.name == category);
            if (cat == null)
            {
                cat = new Category();
                cat.name = category;
                cat.entries = new List<string>();
                list.Add(cat);
            }
            cat.entries.Add(entry);
        }

        return list;
    }
}
