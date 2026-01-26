using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "Data/CharacterProfile", order = 1)]
public class CharacterProfile : ScriptableObject
{
    public string Guid;
    public string characterName;
    public int securityClearance;
    public Sprite avatar;
    public GameObject prefab;
    public EntityMotor motor;


    public void NewGuid()
    {
        Guid = System.Guid.NewGuid().ToString();
    }
}
