using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MaskMenu : SubMenu
{
    public MaskButton maskButton;
    public Transform buttonContainer;

    public MaskButton[] buttons;

    private void OnEnable()
    {
        var masks = new List<CharacterProfile>();
        var maskIds = GameManager.CurrentGameSave.Masks;
        for (var i = 0; i < maskIds.Count; i++)
        {
            var maskProfile = GameManager.AllProfiles.FirstOrDefault(m => m.Guid == maskIds[i]);
            if (maskProfile != null)
                masks.Add(maskProfile);
        }
        PopulateMenu(masks);
    }

    public void PopulateMenu(List<CharacterProfile> masks)
    {
        foreach (var child in buttons)
            Destroy(child.gameObject);

        buttons = new MaskButton[masks.Count];
        for (var i = 0; i < buttons.Length; i++)
        {
            MaskButton buttonObj = Instantiate(maskButton, buttonContainer);
            buttonObj.Setup(masks[i]);
            buttons[i] = buttonObj;
        }
    }
}
