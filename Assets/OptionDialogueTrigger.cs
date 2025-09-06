using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionDialogueTrigger : Interactable
{
    [Header("Dialogue Data")]
    public OptionDialogueDefinition dialogueData;

    [Header("Trigger Settings")]
    public bool canTriggerMultipleTimes = true;
    public bool hasBeenTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }

    void TriggerDialogue()
    {
        if (GameManager.instance.dialogueActionHandler != null && GameManager.instance.dialogueActionHandler.HasQuizStarted())
        {
            Debug.Log("Cannot trigger option dialogue - quiz has started");
            return;
        }

        if (!canTriggerMultipleTimes && hasBeenTriggered)
            return;

        OptionDialogueSystem dialogueSystem = GameManager.instance.optionDialogueSystem;
        if (dialogueSystem != null)
        {
            dialogueSystem.Initialize(dialogueData);
            hasBeenTriggered = true;
        }
    }

    public override void Interact(Character character)
    {
        TriggerDialogue();
    }
}