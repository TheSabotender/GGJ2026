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

    public Rigidbody2D Rigidbody => rigidbody;

    public void Setup(MaskState state, CharacterProfile profile)
    {
        if (state == null || profile == null)
            return;
        maskState = state;
        characterProfile = profile;

        image.enabled = characterProfile.mask != null;
        image.sprite = characterProfile.mask;
    }

    public void OnMouseOver()
    {
        MaskDetailPanel.Setup(characterProfile);
    }

    public void OnClick()
    {
        MenuManager.SetScreen(MenuManager.Screen.None);
        GameManager.PlayerBrain.SwapMask(maskState, characterProfile);
    }
}
