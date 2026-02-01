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

    private CharacterProfile currentProfile;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Setup(CharacterProfile profile)
    {
        if (profile == currentProfile)
            return;

        currentProfile = profile;
        gameObject.SetActive(profile != null);

        if (profile != null)
            UpdateData();
    }

    public void UpdateData()
    {
        if (currentProfile == null)
            return;

        portrait.enabled = currentProfile.portrait != null;
        portrait.sprite = currentProfile.portrait;
        characterName.text = currentProfile.characterName;

        if (LocalizationManager.TryGetValue("job-" + currentProfile.field.ToString().ToLower(), out var localizedField))
            field.text = localizedField;
        else
            field.text = currentProfile.field.ToString();

        if (LocalizationManager.TryGetValue("clearance-" + currentProfile.securityClearance.ToString().ToLower(), out var localizedClearance))
            securityClearance.text = localizedClearance;
        else
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

                if(LocalizationManager.TryGetValue("interest-" + flag.ToString().ToLower(), out var localizedFlag))
                    l += localizedFlag;
                else
                    l += flag.ToString();
            }
        }

        if(l.Length > 0)
            return l.Substring(0, l.Length - 1);

        return string.Empty;
    }
}
