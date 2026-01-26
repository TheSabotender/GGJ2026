using UnityEngine;

public class SubMenu : MonoBehaviour
{
    [SerializeField]
    private MenuScreen screen = MenuScreen.None;

    public MenuScreen Screen => screen;

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
