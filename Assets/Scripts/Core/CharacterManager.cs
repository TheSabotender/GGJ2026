using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public CharacterProfile[] characterProfiles;

    public CharacterProfile FindProfile(string guid)
    {
        foreach (var profile in characterProfiles)
        {
            if (profile.Guid == guid)
                return profile;
        }
        return null;
    }
}
