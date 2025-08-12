using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueActionHandler : MonoBehaviour
{
    [Header("Global Dialogue Actions")]
    [SerializeField] private List<DialogueActionSet> dialogueActionSets = new List<DialogueActionSet>();

    // Dictionary for quick lookup
    private Dictionary<string, DialogueActionSet> actionSetLookup = new Dictionary<string, DialogueActionSet>();

    private void Start()
    {
        // Build lookup dictionary
        BuildActionLookup();
    }

    private void BuildActionLookup()
    {
        actionSetLookup.Clear();
        foreach (var actionSet in dialogueActionSets)
        {
            if (!string.IsNullOrEmpty(actionSet.actionSetId))
            {
                actionSetLookup[actionSet.actionSetId] = actionSet;
            }
        }
    }

    public void ExecuteOptionAction(string actionSetId, int optionIndex)
    {
        if (actionSetLookup.TryGetValue(actionSetId, out DialogueActionSet actionSet))
        {
            if (optionIndex >= 0 && optionIndex < actionSet.optionActions.Count)
            {
                actionSet.optionActions[optionIndex]?.Invoke();
            }
            else
            {
                Debug.LogWarning($"Option index {optionIndex} out of range for action set {actionSetId}");
            }
        }
        else
        {
            Debug.LogWarning($"Action set with ID '{actionSetId}' not found");
        }
    }

    // Helper method to get action set by ID
    public DialogueActionSet GetActionSet(string actionSetId)
    {
        actionSetLookup.TryGetValue(actionSetId, out DialogueActionSet actionSet);
        return actionSet;
    }

    // Called when inspector values change
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            BuildActionLookup();
        }
    }
}

[System.Serializable]
public class DialogueActionSet
{
    [Header("Action Set Info")]
    public string actionSetId = ""; // Unique identifier for this action set
    public string description = ""; // Optional description for organization

    [Header("Option Actions")]
    public List<UnityEvent> optionActions = new List<UnityEvent>();

    // Helper method to ensure the action list matches a specific count
    public void ResizeActionList(int targetCount)
    {
        while (optionActions.Count < targetCount)
        {
            optionActions.Add(new UnityEvent());
        }

        while (optionActions.Count > targetCount)
        {
            optionActions.RemoveAt(optionActions.Count - 1);
        }
    }
}