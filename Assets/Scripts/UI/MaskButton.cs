using UnityEngine;
using UnityEngine.UI;

public class MaskButton : MonoBehaviour
{
    [SerializeField]
    private Image image;

    [SerializeField]
    new private Rigidbody2D rigidbody;

    private MaskState maskState;
    private CharacterProfile characterProfile;
    private MaskMenu maskMenu;

    public Rigidbody2D Rigidbody => rigidbody;

    public void Setup(MaskState state, CharacterProfile profile, MaskMenu maskMenu)
    {
        this.maskMenu = maskMenu;
        if (state == null || profile == null)
            return;
        maskState = state;
        characterProfile = profile;

        image.enabled = characterProfile.mask != null;
        image.sprite = characterProfile.mask;
    }

    public void OnMouseOver()
    {
        maskMenu.DetailPanel.Setup(characterProfile);
    }

    public void OnClick()
    {
        MenuManager.SetScreen(MenuManager.Screen.None);
        GameSceneManager.PlayerBrain.SwapMask(maskState, characterProfile);
    }
}
