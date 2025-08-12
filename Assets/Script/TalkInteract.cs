using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkInteract : Interactable
{
    //[SerializeField] DialogueContainer dialogue;

    NPCCharacter npcCharacter;
    NPCDefinition nPCDefinition;

    private void Awake()
    {
        npcCharacter = GetComponent<NPCCharacter>();
        nPCDefinition = GetComponent<NPCCharacter>().character;
    }

    public override void Interact(Character character)
    {
        DialogueContainer dialogueContainer = nPCDefinition.generalDialogues[Random.Range(0, nPCDefinition.generalDialogues.Count)];
        npcCharacter.IncreaseRelationship(1);
        GameManager.instance.dialogueSystem.Initialize(dialogueContainer);
    }
}
