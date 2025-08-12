using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInteract : MonoBehaviour
{
    CharacterController2D characterController;
    Rigidbody2D rgbd2d;
    [SerializeField] float offsetDistance = 1f;
    [SerializeField] float sizeOfInteractableArea = 1.2f;
    Character character;
    [SerializeReference] HighlightController highlightController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController2D>();
        rgbd2d = GetComponent<Rigidbody2D>();
        character = GetComponent<Character>();
    }

    private void Update()
    {
        Check();
        if (Input.GetMouseButtonDown(1))
        {
            Interact();
        }
    }

    public void Check()
    {
        // Check for interactables around the character
        Interactable foundInteractable = FindNearestInteractable();

        if (foundInteractable != null)
        {
            highlightController.Highlight(foundInteractable.gameObject);
        }
        else
        {
            highlightController.Hide();
        }
    }

    private void Interact()
    {
        // Find and interact with the nearest interactable
        Interactable foundInteractable = FindNearestInteractable();

        if (foundInteractable != null)
        {
            foundInteractable.Interact(character);
        }
    }

    private Interactable FindNearestInteractable()
    {
        // Check both at character position and offset position
        Vector2 characterPosition = rgbd2d.position;
        Vector2 offsetPosition = rgbd2d.position + characterController.lastMotionVector * offsetDistance;

        // First check at character's actual position
        Collider2D[] colliders = Physics2D.OverlapCircleAll(characterPosition, sizeOfInteractableArea);
        Interactable nearestInteractable = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D c in colliders)
        {
            Interactable interactable = c.GetComponent<Interactable>();
            if (interactable != null)
            {
                float distance = Vector2.Distance(characterPosition, c.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestInteractable = interactable;
                }
            }
        }

        // If no interactable found at character position, check offset position
        if (nearestInteractable == null)
        {
            colliders = Physics2D.OverlapCircleAll(offsetPosition, sizeOfInteractableArea);
            foreach (Collider2D c in colliders)
            {
                Interactable interactable = c.GetComponent<Interactable>();
                if (interactable != null)
                {
                    float distance = Vector2.Distance(characterPosition, c.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestInteractable = interactable;
                    }
                }
            }
        }

        return nearestInteractable;
    }
}