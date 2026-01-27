using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "Data/CharacterProfile", order = 1)]
public class CharacterProfile : ScriptableObject
{
    [Header("Core")]
    public string Guid;
    public EntityMotor motor;

    [Header("Visuals")]
    public GameObject prefab;
    public Sprite portrait;
    public Sprite mask;

    [Header("Meta")]
    public string characterName;
    public JobTag field;
    public Clearance securityClearance;
    public string description;
    public InterestTag likes;

    public void NewGuid()
    {
        Guid = System.Guid.NewGuid().ToString();
    }

    public enum Clearance
    {
        Alien = -99,
        Wanted = -10,
        Criminal = -1,
        Civilian = 0,
        Employee = 1,
        Valued = 2,
        Official = 3,
        Executive = 4
    }

    public enum JobTag
    {
        None,
        Scientist,
        Engineer,
        Security,
        Medical,
        Administrative,
        Maintenance
    }

    [Flags]
    public enum InterestTag
    {
        None = 0,
        Technology = 1 << 0,
        Engineering = 1 << 1,
        Alcohol = 1 << 2,
        Lust = 1 << 3,
        Money = 1 << 4,
    }
}
