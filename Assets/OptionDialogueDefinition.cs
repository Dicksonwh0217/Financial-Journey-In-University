using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Dialogue/Option Dialogue")]
public class OptionDialogueDefinition : ScriptableObject
{
    [Header("Display")]
    public string characterName = "System";
    public Sprite portrait;
    public string questionText = "What will you do?";

    [Header("Actions")]
    [Tooltip("ID of the action set in GameManager's DialogueActionHandler")]
    public string actionSetId = "";

    [Header("Options")]
    public List<DialogueOption> options;
}

[System.Serializable]
public class DialogueOption
{
    public string optionText;
    public OptionType optionType = OptionType.Normal;

    [Header("Requirements (Optional)")]
    public bool hasRequirements;
    public List<OptionRequirement> requirements;

    [Header("Actions")]
    public bool closesDialogue = true;
}

public enum OptionType
{
    Normal,
    Skill,
    Risky,
    Positive,
    Locked
}

[System.Serializable]
public class OptionRequirement
{
    public OptionType type;
    public string requirementKey;
    public int requiredValue;
}