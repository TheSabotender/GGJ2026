using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Image portrait;
    public TMPro.TextMeshProUGUI profileName;
    public Image health;

    private Material healthMaterial;
    private string lastMaskGuid;

    private void Awake()
    {
        canvasGroup.alpha = 0;

        healthMaterial = Instantiate(health.material);
        health.material = healthMaterial;
    }

    private void Update()
    {
        bool isGameplay = GameManager.CurrentGameSave != null;

        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, isGameplay ? 1 : 0, Time.deltaTime);

        if (isGameplay)
        {
            healthMaterial.SetFloat("_Health", 1);
            SetMask();
        }
    }

    void SetMask()
    {
        var currentMaskGuid = GameManager.CurrentGameSave.CurrentMask;
        if (currentMaskGuid == lastMaskGuid)
            return;
        lastMaskGuid = currentMaskGuid;

        if (string.IsNullOrWhiteSpace(currentMaskGuid))
            return;

        var currentMask = GameManager.AllProfiles.FirstOrDefault(p => p.Guid == currentMaskGuid);
        if (currentMask == null)
            return;

        portrait.enabled = currentMask.portrait != null;
        portrait.sprite = currentMask.portrait;

        profileName.enabled = !string.IsNullOrWhiteSpace(currentMask.characterName);
        profileName.text = currentMask.characterName;
    }
}
