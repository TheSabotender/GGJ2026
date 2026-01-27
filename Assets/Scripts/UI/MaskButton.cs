using UnityEngine;
using UnityEngine.UI;

public class MaskButton : MonoBehaviour
{
    [SerializeField]
    private Image image;

    private CharacterProfile characterProfile;
    private Rigidbody2D rigidbody;

    public Rigidbody2D Rigidbody => rigidbody ??= image.GetComponent<Rigidbody2D>();

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
        //TODO set tooltip info
    }

    public void OnClick()
    {
        MenuManager.SetScreen(MenuManager.Screen.None);
        GameManager.PlayerBrain.SwapMask(characterProfile);
    }
}
