using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Image[] alertColorImages;
    public Image portrait;
    public TMPro.TextMeshProUGUI profileName;
    public Image health;
    public float hudFadeSpeed = 1;

    private Color baseColor;
    private Material healthMaterial;
    private string lastMaskGuid;

    private void Awake()
    {
        canvasGroup.alpha = 0;
        baseColor = alertColorImages.Length > 0 ? alertColorImages[0].color : Color.white;

        healthMaterial = Instantiate(health.material);
        health.material = healthMaterial;
        RegionManager.AlertStateChanged += OnAlertStateChanged;
    }

    private void OnAlertStateChanged(GameManager.AlertState newAlert)
    {
        var color = newAlert switch
        {
            GameManager.AlertState.Normal => baseColor,
            GameManager.AlertState.Caution => Color.yellow,
            GameManager.AlertState.Alert => Color.red,
            _ => Color.white
        };

        foreach (var img in alertColorImages)
            img.color = color;
    }

    private void Update()
    {
        bool isGameplay = MenuManager.CurrentScreen == MenuManager.Screen.None;

        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, isGameplay ? 1 : 0, Time.deltaTime * hudFadeSpeed);

        if (isGameplay)
        {
            healthMaterial.SetFloat("_Health", 1);
            SetMask();
        }
    }

    void SetMask()
    {
        var currentMaskGuid = GameManager.CurrentGameSave.Masks[GameManager.CurrentGameSave.CurrentMask];
        if (currentMaskGuid == null || currentMaskGuid.guid == lastMaskGuid)
            return;
        lastMaskGuid = currentMaskGuid.guid;

        var currentMask = GameManager.AllProfiles.FirstOrDefault(p => p.Guid == lastMaskGuid);
        if (currentMask == null)
            return;

        portrait.enabled = currentMask.portrait != null;
        portrait.sprite = currentMask.portrait;

        profileName.enabled = !string.IsNullOrWhiteSpace(currentMask.characterName);
        profileName.text = currentMask.characterName;
    }
}
