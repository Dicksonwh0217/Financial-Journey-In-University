using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Item")]
public class Item : ScriptableObject
{
    public enum ItemType
    {
        Food,
        Drink,
        Item,
        Tool,
        Stock
    }

    public string Name;
    public bool stackable;
    [TextArea(3, 5)]
    public string description;
    public ItemType itemType = ItemType.Item;
    public Sprite icon;
    public ToolAction onAction;
    public ToolAction onTileMapAction;
    public ToolAction onItemUsed;
    public bool itemIconHighlight;
    public GameObject itemPrefab;
    public int price;
    public bool canBeSold = true;

    [Header("Consumable Properties")]
    public bool consumable;
    [SerializeField] int hungerRestoreAmount = 10;
    [SerializeField] int thirstRestoreAmount = 0; // New thirst restore property
    [SerializeField] int healthRestoreAmount = 0;
    [SerializeField] int happinessRestoreAmount = 0;

    // Public getters for the restore amounts
    public int HungerRestoreAmount => hungerRestoreAmount;
    public int ThirstRestoreAmount => thirstRestoreAmount; // New getter
    public int HealthRestoreAmount => healthRestoreAmount;
    public int HappinessRestoreAmount => happinessRestoreAmount;
}