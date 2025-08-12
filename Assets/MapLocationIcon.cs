using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapLocationIcon : MonoBehaviour
{
    [Header("Location Settings")]
    [SerializeField] private string locationName;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] private Sprite locationIcon;
    [Header("UI References")]
    [SerializeField] private Button locationButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI locationLabel;
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private float scaleOnHover = 1.1f;
    [Header("Player UI")]
    private Vector3 originalScale;
    private MapManager mapManager;
    void Start()
    {
        mapManager = FindFirstObjectByType<MapManager>();
        originalScale = transform.localScale;
        // Setup UI components
        if (locationButton == null)
            locationButton = GetComponent<Button>();
        if (iconImage == null)
            iconImage = GetComponent<Image>();
        if (locationLabel == null)
            locationLabel = GetComponentInChildren<TextMeshProUGUI>();
        // Configure the button
        if (locationButton != null)
        {
            locationButton.onClick.AddListener(OnLocationClicked);
        }
        // Set initial values
        if (iconImage != null && locationIcon != null)
            iconImage.sprite = locationIcon;
        if (locationLabel != null)
            locationLabel.text = locationName;
    }
    public void OnLocationClicked()
    {
        if (mapManager != null)
        {
            mapManager.TravelToLocation(sceneToLoad, spawnPosition);
        }
        else
        {
            // Fallback if no map manager found
            GameSceneManager.instance.InitSwitchScene(sceneToLoad, spawnPosition);
        }
    }
    public void OnPointerEnter()
    {
        // Visual feedback on hover
        if (iconImage != null)
            iconImage.color = hoverColor;
        transform.localScale = originalScale * scaleOnHover;
    }
    public void OnPointerExit()
    {
        // Return to normal state
        if (iconImage != null)
            iconImage.color = normalColor;
        transform.localScale = originalScale;
    }
    // Method to setup location data programmatically
    public void SetupLocation(string name, string scene, Vector3 spawn, Sprite icon = null)
    {
        locationName = name;
        sceneToLoad = scene;
        spawnPosition = spawn;
        if (icon != null)
            locationIcon = icon;
        // Update UI if components exist
        if (locationLabel != null)
            locationLabel.text = locationName;
        if (iconImage != null && locationIcon != null)
            iconImage.sprite = locationIcon;
    }

}