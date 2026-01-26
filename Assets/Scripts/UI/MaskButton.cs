using UnityEngine;
using UnityEngine.UI;

public class MaskButton : MonoBehaviour
{
    [SerializeField]
    private Image image;

    private CharacterProfile characterProfile;

    public void Setup(CharacterProfile profile)
    {
        characterProfile = profile;

        if (image != null)
            image.sprite = characterProfile.avatar;
    }

    public void OnMouseOver()
    {
        //TODO set tooltip info
    }

    public void OnClick()
    {
        GameManager.PlayerBrain.SwapMask(characterProfile);
    }
}
