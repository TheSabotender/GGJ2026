using UnityEngine;
using UnityEngine.UI;

public class MaskDetailPanel : MonoBehaviour
{
    public Image portrait;
    public TMPro.TextMeshProUGUI characterName;
    public TMPro.TextMeshProUGUI field;
    public TMPro.TextMeshProUGUI securityClearance;
    public TMPro.TextMeshProUGUI description;
    public TMPro.TextMeshProUGUI likes;

    private static MaskDetailPanel instance;
    private CharacterProfile currentProfile;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public static void Setup(CharacterProfile profile)
    {
        if (profile == instance.currentProfile)
            return;

        instance.currentProfile = profile;
        instance.gameObject.SetActive(profile != null);

        if (profile != null)
            instance.UpdateData();
    }

    public void UpdateData()
    {
        if (currentProfile == null)
            return;

        portrait.enabled = currentProfile.portrait != null;
        portrait.sprite = currentProfile.portrait;
        characterName.text = currentProfile.characterName;
        field.text = currentProfile.field.ToString();
        securityClearance.text = currentProfile.securityClearance.ToString();
        description.text = currentProfile.description;

        likes.text = BitFieldToString<CharacterProfile.InterestTag>(currentProfile.likes);
    }

    public static string BitFieldToString<TEnum>(TEnum value)
    {
        var l = string.Empty;

        foreach (TEnum flag in System.Enum.GetValues(typeof(TEnum)))
        {
            int intFlag = System.Convert.ToInt32(flag);
            int intValue = System.Convert.ToInt32(value);
            if (intFlag != 0 && (intValue & intFlag) == intFlag)
            {
                if (l.Length > 0)
                    l += ", ";
                l += flag.ToString();
            }
        }

        if(l.Length > 0)
            return l.Substring(0, l.Length - 1);

        return string.Empty;
    }
}
