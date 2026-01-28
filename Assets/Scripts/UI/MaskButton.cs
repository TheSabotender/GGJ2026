using UnityEngine;
using UnityEngine.UI;

public class MaskButton : MonoBehaviour
{
    [SerializeField]
    private Image image;

    [SerializeField]
    new private Rigidbody2D rigidbody;

    private CharacterProfile characterProfile;

    public Rigidbody2D Rigidbody => rigidbody;

    public void Setup(CharacterProfile profile)
    {
        if (profile == null)
            return;
        characterProfile = profile;

        Debug.Log($"Setting mask button to {profile?.name} with mask {characterProfile?.mask}");

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
        GameManager.PlayerBrain.SwapMask(characterProfile);
    }
}
