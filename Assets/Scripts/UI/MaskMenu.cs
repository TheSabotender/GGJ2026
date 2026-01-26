using System.Collections.Generic;
using UnityEngine;

public class MaskMenu : SubMenu
{
    public MaskButton maskButton;
    public Transform buttonContainer;

    public MaskButton[] buttons;

    public void PopulateMenu(List<CharacterProfile> masks)
    {
        foreach (var child in buttons)
            Destroy(child.gameObject);

        foreach (var mask in masks)
        {
            MaskButton buttonObj = Instantiate(maskButton, buttonContainer);
            buttonObj.Setup(mask);
        }
    }
}
