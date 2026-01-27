using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Localized : MonoBehaviour
{
    [SerializeField]
    private string localizationKey;

    private TextMeshProUGUI textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    public void RefreshLocalization()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        if (textComponent == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(localizationKey))
        {
            textComponent.text = string.Empty;
            return;
        }

        if (LocalizationManager.TryGetValue(localizationKey, out var value))
        {
            textComponent.text = Colorize(value);
        }
        else
        {
            textComponent.text = localizationKey;
        }
    }

    string Colorize(string input)
    {
        var prefix = "<color=red>";
        var suffix = "</color>";

        var text = string.Empty;
        bool wasSpace = true;
        for (int i = 0; i < input.Length; i++)
        {
            if (wasSpace)
                text += prefix;

            text += input[i];

            if (wasSpace)
                text += suffix;

            wasSpace = input[i] == ' ';
        }
        return text;
    }
}
