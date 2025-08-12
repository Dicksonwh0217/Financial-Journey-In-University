using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum StatType
{
    Hunger,
    Health,
    Happiness,
    Thirst,
    All // Shows all stats
}

public class StatPanelHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private GameObject characterStatusDetail;
    [SerializeField] private Text statusText;
    [SerializeField] private Character character;

    [Header("Stat Configuration")]
    [SerializeField] private StatType statToShow = StatType.Hunger;

    [Header("Mouse Follow Settings")]
    [SerializeField] private Vector2 offset = new Vector2(10, -10);
    [SerializeField] private bool followMouse = true;

    [Header("Optional Settings")]
    [SerializeField] private float showDelay = 0.1f;
    [SerializeField] private float hideDelay = 0.0f;
    [SerializeField] private float updateInterval = 0.1f; // Update text every 0.1 seconds

    private Coroutine showCoroutine;
    private Coroutine hideCoroutine;
    private bool isHovering = false;
    private RectTransform statusPanelRect;
    private float lastUpdateTime = 0f;

    private void Start()
    {
        if (characterStatusDetail != null)
        {
            statusPanelRect = characterStatusDetail.GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        if (isHovering && characterStatusDetail != null && characterStatusDetail.activeInHierarchy)
        {
            // Update position if following mouse
            if (followMouse)
            {
                UpdateStatusPanelPosition();
            }

            // Update text at intervals to show current values
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateStatusText();
                lastUpdateTime = Time.time;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (characterStatusDetail != null)
        {
            showCoroutine = StartCoroutine(ShowDetailWithDelay());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        if (characterStatusDetail != null)
        {
            hideCoroutine = StartCoroutine(HideDetailWithDelay());
        }
    }

    private IEnumerator ShowDetailWithDelay()
    {
        if (showDelay > 0)
        {
            yield return new WaitForSeconds(showDelay);
        }

        if (characterStatusDetail != null && isHovering)
        {
            UpdateStatusText();
            characterStatusDetail.SetActive(true);
            UpdateStatusPanelPosition();
            lastUpdateTime = Time.time; // Reset update timer
        }
    }

    private IEnumerator HideDetailWithDelay()
    {
        if (hideDelay > 0)
        {
            yield return new WaitForSeconds(hideDelay);
        }

        if (characterStatusDetail != null)
        {
            characterStatusDetail.SetActive(false);
        }
    }

    private void UpdateStatusPanelPosition()
    {
        if (statusPanelRect == null)
            return;

        // Get mouse position in screen coordinates
        Vector3 mousePosition = Input.mousePosition;

        // Add offset
        mousePosition += new Vector3(offset.x, offset.y, 0);

        // For UI elements, we can directly use screen position
        statusPanelRect.position = mousePosition;
    }

    private void UpdateStatusText()
    {
        if (statusText != null && character != null)
        {
            string statusInfo = "";

            if (statToShow == StatType.All)
            {
                statusInfo = $"Hunger : {character.Hunger.currVal}/{character.Hunger.maxVal}\n";
                statusInfo += $"Health : {character.Health.currVal}/{character.Health.maxVal}\n";
                statusInfo += $"Happiness : {character.Happiness.currVal}/{character.Happiness.maxVal}\n";
                statusInfo += $"Thirst : {character.Thirst.currVal}/{character.Thirst.maxVal}";
            }
            else
            {
                switch (statToShow)
                {
                    case StatType.Hunger:
                        statusInfo = $"Hunger : {character.Hunger.currVal}/{character.Hunger.maxVal}";
                        break;
                    case StatType.Health:
                        statusInfo = $"Health : {character.Health.currVal}/{character.Health.maxVal}";
                        break;
                    case StatType.Happiness:
                        statusInfo = $"Happiness : {character.Happiness.currVal}/{character.Happiness.maxVal}";
                        break;
                    case StatType.Thirst:
                        statusInfo = $"Thirst : {character.Thirst.currVal}/{character.Thirst.maxVal}";
                        break;
                }
            }

            statusText.text = statusInfo;
        }
    }

    private void OnDisable()
    {
        isHovering = false;

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (characterStatusDetail != null)
        {
            characterStatusDetail.SetActive(false);
        }
    }
}