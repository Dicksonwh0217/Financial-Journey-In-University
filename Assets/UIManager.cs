using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements to Hide in BigMap")]
    [SerializeField] private List<GameObject> uiElementsToHide = new List<GameObject>();

    [Header("Auto-Find UI Elements")]
    [SerializeField] private bool autoFindUIElements = true;
    [SerializeField] private string[] uiTagsToHide = { "GameUI", "HUD", "PlayerUI" };

    [Header("UI State")]
    [SerializeField] private bool isInBigMap = false;

    public static UIManager instance;

    private Dictionary<GameObject, bool> originalUIStates = new Dictionary<GameObject, bool>();

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (autoFindUIElements)
        {
            FindUIElements();
        }

        // Store original states
        StoreOriginalUIStates();
    }

    private void FindUIElements()
    {
        // Find UI elements by tags
        foreach (string tag in uiTagsToHide)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in taggedObjects)
            {
                if (!uiElementsToHide.Contains(obj))
                {
                    uiElementsToHide.Add(obj);
                }
            }
        }

        // Find common UI elements by name patterns
        string[] commonUINames = { "ToolBarPanel", "CharacterStatusPanel", "InventoryPanel",
                                  "ContainerPanel", "StorePanel", "DialogueIcon", "DialoguePanel",
                                  "TintScreenImage", "TimePanel", "DayPanel" };

        foreach (string uiName in commonUINames)
        {
            GameObject found = GameObject.Find(uiName);
            if (found != null && !uiElementsToHide.Contains(found))
            {
                uiElementsToHide.Add(found);
            }
        }
    }

    private void StoreOriginalUIStates()
    {
        originalUIStates.Clear();
        foreach (GameObject uiElement in uiElementsToHide)
        {
            if (uiElement != null)
            {
                originalUIStates[uiElement] = uiElement.activeInHierarchy;
            }
        }
    }

    public void SetBigMapMode(bool inBigMap)
    {
        isInBigMap = inBigMap;

        if (inBigMap)
        {
            HideUIElements();
        }
        else
        {
            ShowUIElements();
        }
    }

    private void HideUIElements()
    {
        foreach (GameObject uiElement in uiElementsToHide)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(false);
            }
        }

        Debug.Log("UI elements hidden for BigMap mode");
    }

    private void ShowUIElements()
    {
        foreach (GameObject uiElement in uiElementsToHide)
        {
            if (uiElement != null && originalUIStates.ContainsKey(uiElement))
            {
                uiElement.SetActive(originalUIStates[uiElement]);
            }
        }

        Debug.Log("UI elements restored from BigMap mode");
    }

    // Method to manually add UI elements to hide
    public void AddUIElementToHide(GameObject uiElement)
    {
        if (uiElement != null && !uiElementsToHide.Contains(uiElement))
        {
            uiElementsToHide.Add(uiElement);
            originalUIStates[uiElement] = uiElement.activeInHierarchy;
        }
    }

    // Method to remove UI elements from hide list
    public void RemoveUIElementToHide(GameObject uiElement)
    {
        if (uiElement != null)
        {
            uiElementsToHide.Remove(uiElement);
            originalUIStates.Remove(uiElement);
        }
    }

    // Refresh UI elements list (useful when UI is dynamically created)
    public void RefreshUIElements()
    {
        if (autoFindUIElements)
        {
            uiElementsToHide.Clear();
            FindUIElements();
            StoreOriginalUIStates();
        }
    }

    // Get current state
    public bool IsInBigMap()
    {
        return isInBigMap;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}