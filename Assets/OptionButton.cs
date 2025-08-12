using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI optionText;
    [SerializeField] Button button;

    [Header("Text Colors")]
    [SerializeField] Color normalTextColor = Color.white;
    [SerializeField] Color disabledTextColor = Color.grey;

    private void Awake()
    {
        // Store the original text color if not set in inspector
        if (optionText != null && normalTextColor == Color.white)
        {
            normalTextColor = optionText.color;
        }
    }

    public void SetOptionText(string text)
    {
        optionText.text = text;
    }

    public void SetButtonInteractable(bool interactable)
    {
        button.interactable = interactable;

        // Change text color based on interactable state
        if (optionText != null)
        {
            optionText.color = interactable ? normalTextColor : disabledTextColor;
        }
    }

    // Optional: Set different colors for different option types
    public void SetOptionType(OptionType type)
    {
        ColorBlock colors = button.colors;
        switch (type)
        {
            case OptionType.Normal:
                // Keep default colors
                break;
            case OptionType.Skill:
                colors.normalColor = Color.cyan;
                // Also update the normal text color for this type if needed
                if (button.interactable)
                {
                    normalTextColor = Color.cyan;
                    optionText.color = normalTextColor;
                }
                break;
            case OptionType.Risky:
                colors.normalColor = Color.red;
                if (button.interactable)
                {
                    normalTextColor = Color.red;
                    optionText.color = normalTextColor;
                }
                break;
            case OptionType.Positive:
                colors.normalColor = Color.green;
                if (button.interactable)
                {
                    normalTextColor = Color.green;
                    optionText.color = normalTextColor;
                }
                break;
            case OptionType.Locked:
                colors.normalColor = Color.gray;
                // Locked options should always appear grey
                normalTextColor = Color.gray;
                optionText.color = Color.gray;
                break;
        }
        button.colors = colors;
    }

    // Optional: Method to manually set text colors
    public void SetTextColors(Color normal, Color disabled)
    {
        normalTextColor = normal;
        disabledTextColor = disabled;

        // Update current text color based on current interactable state
        if (optionText != null)
        {
            optionText.color = button.interactable ? normalTextColor : disabledTextColor;
        }
    }
}