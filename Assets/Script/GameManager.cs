using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private void Awake()
    {
        instance = this;
    }
    public GameObject player;
    public ItemContainer inventoryContainer;
    public ItemDragAndDropController dragAndDropController;
    public DialogueSystem dialogueSystem;
    public DayTime timeController;
    public ScreenTint screenTint;
    public PlaceableObjectsReferenceManager placeableObjects;
    public ItemStockPanel stockStorePanel;
    public OptionDialogueSystem optionDialogueSystem;
    public DialogueActionHandler dialogueActionHandler;
    public AchievementManager achievementManager;
}