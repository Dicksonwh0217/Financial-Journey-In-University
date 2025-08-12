using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Gender
{
    Male,
    Female,
    [InspectorName("Any [For romance only]")]
    Any
}

[Serializable]
public class ProtraitsCollection
{
    public Texture2D normal;
    public Texture2D surprised;
    public Texture2D confused;
    public Texture2D angry;
}

[CreateAssetMenu(menuName = "Data/NPC Character")]
public class NPCDefinition : ScriptableObject
{
    public string Name = "Name";
    public Gender gender = Gender.Male;

    public ProtraitsCollection protraits;

    public GameObject characterPrefab;

    [Header("Interaction")]
    public bool canBeRomanced;
    public Gender romancableGender;

    public List<Item> itemLikes;
    public List<Item> itemDislikes;

    [Header("Dialogues")]
    public List<DialogueContainer> generalDialogues;

    [Header("Schedule WIP")]
    public string schedule;
}
