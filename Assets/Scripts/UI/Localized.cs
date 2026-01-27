using TMPro;
using UnityEngine;

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

    private void OnValidate()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        UpdateText();
    }

    private void UpdateText()
    {
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
            textComponent.text = value;
        }
        else
        {
            textComponent.text = localizationKey;
        }
    }
}
