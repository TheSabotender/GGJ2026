using System.Collections;
using UnityEngine;

public class SubMenu : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.25f;

    public virtual IEnumerator Show()
    {
        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }

    public virtual IEnumerator Hide()
    {
        if (canvasGroup != null) {
            canvasGroup.alpha = 1f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        gameObject.SetActive(false);
    }
}
