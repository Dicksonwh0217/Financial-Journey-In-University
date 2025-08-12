using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LocationData
{
    public string locationName;
    public string sceneName;
    public Vector3 spawnPosition;
    public Sprite locationIcon;
    public Vector2 mapPosition; // Position on the map UI
    public bool isUnlocked = true;
}

public class MapManager : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private List<LocationData> availableLocations = new List<LocationData>();
    [SerializeField] private GameObject locationIconPrefab;
    [SerializeField] private Transform mapContainer; // Parent object for map icons

    [Header("Player Movement on Map")]
    [SerializeField] private bool allowPlayerMovementOnMap = false;
    [SerializeField] private CharacterController2D playerController;

    private void Start()
    {
        SetupMapIcons();
        ConfigurePlayerForMap();
    }

    private void SetupMapIcons()
    {
        // Clear existing icons
        foreach (Transform child in mapContainer)
        {
            if (child.GetComponent<MapLocationIcon>() != null)
                DestroyImmediate(child.gameObject);
        }

        // Create icons for each location
        foreach (LocationData location in availableLocations)
        {
            if (location.isUnlocked)
            {
                CreateLocationIcon(location);
            }
        }
    }

    private void CreateLocationIcon(LocationData location)
    {
        if (locationIconPrefab == null || mapContainer == null)
        {
            Debug.LogError("MapManager: Missing locationIconPrefab or mapContainer reference!");
            return;
        }

        GameObject iconObj = Instantiate(locationIconPrefab, mapContainer);
        iconObj.transform.localPosition = location.mapPosition;

        MapLocationIcon iconScript = iconObj.GetComponent<MapLocationIcon>();
        if (iconScript != null)
        {
            iconScript.SetupLocation(
                location.locationName,
                location.sceneName,
                location.spawnPosition,
                location.locationIcon
            );
        }
    }

    private void ConfigurePlayerForMap()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<CharacterController2D>();

        if (playerController != null)
        {
            // Enable or disable player movement based on settings
            playerController.enabled = allowPlayerMovementOnMap;
        }
    }

    public void TravelToLocation(string sceneName, Vector3 spawnPosition)
    {
        StartCoroutine(TravelToLocationCoroutine(sceneName, spawnPosition));
    }

    public void UnlockLocation(string locationName)
    {
        LocationData location = availableLocations.Find(loc => loc.locationName == locationName);
        if (location != null && !location.isUnlocked)
        {
            location.isUnlocked = true;
            CreateLocationIcon(location);
        }
    }

    public void LockLocation(string locationName)
    {
        LocationData location = availableLocations.Find(loc => loc.locationName == locationName);
        if (location != null && location.isUnlocked)
        {
            location.isUnlocked = false;

            // Remove the icon from map
            MapLocationIcon[] icons = mapContainer.GetComponentsInChildren<MapLocationIcon>();
            foreach (MapLocationIcon icon in icons)
            {
                if (icon.name.Contains(locationName))
                {
                    DestroyImmediate(icon.gameObject);
                    break;
                }
            }
        }
    }

    // Add new location at runtime
    public void AddLocation(LocationData newLocation)
    {
        availableLocations.Add(newLocation);
        if (newLocation.isUnlocked)
        {
            CreateLocationIcon(newLocation);
        }
    }

    private IEnumerator TravelToLocationCoroutine(string sceneName, Vector3 spawnPosition)
    {
        Debug.Log($"Traveling to {sceneName} at position {spawnPosition}");

        // Switch scene
        if (GameSceneManager.instance != null)
        {
            GameSceneManager.instance.InitSwitchScene(sceneName, spawnPosition);
        }

        // Tint first
        if (GameSceneManager.instance != null && GameSceneManager.instance.screenTint != null)
        {
            float duration = 1f / GameSceneManager.instance.screenTint.speed + 0.1f;
            yield return new WaitForSeconds(duration);
        }

        // Optional: Activate UI if needed
        GameObject toolBarPanel = GetEssentialGameObject("ToolBar");
        GameObject characterStatusPanel = GetEssentialGameObject("CharacterStatusPanel");

        if (toolBarPanel != null)
            toolBarPanel.SetActive(true);

        if (characterStatusPanel != null)
            characterStatusPanel.SetActive(true);
    }

    GameObject GetEssentialGameObject(string objectName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (scene.name == "Essential")
            {
                foreach (GameObject obj in scene.GetRootGameObjects())
                {
                    GameObject found = FindInChildrenRecursive(obj.transform, objectName);
                    if (found != null)
                        return found;
                }
            }
        }
        return null;
    }

    GameObject FindInChildrenRecursive(Transform parent, string name)
    {
        if (parent.name == name)
            return parent.gameObject;

        foreach (Transform child in parent)
        {
            GameObject found = FindInChildrenRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}