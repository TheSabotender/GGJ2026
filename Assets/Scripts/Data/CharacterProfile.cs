using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "Data/CharacterProfile", order = 1)]
public class CharacterProfile : ScriptableObject
{
    [Header("Core")]
    public string Guid;
    public EntityMotor motor;

    [Header("Visuals")]
    public CharacterPrefab prefab;
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
        Disliked = -1,
        Civilian = 0,
        Employee = 1,
        Valued = 2,
        Official = 3,
        Executive = 4
    }

    [Flags]
    public enum JobTag
    {
        None = 0,
        Scientist = 1 << 0,
        Engineer = 1 << 1,
        Security = 1 << 2,
        Medical = 1 << 3,
        Administrative = 1 << 4,
        Maintenance = 1 << 5,
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
