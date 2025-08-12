using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZDepth : MonoBehaviour
{
    Transform t;
    [SerializeField] bool stationary = true;
    [SerializeField] bool useBottomOfSprite = false; // New option for proper depth sorting
    [SerializeField] float yOffset = 0f; // Manual Y offset for sorting position

    // For multi-part objects (like your character)
    private SpriteRenderer[] allRenderers;
    private int[] originalOrderInLayer;
    private bool isMultiPart = false;
    private float spriteBottom = 0f; // Cache the bottom position

    private void Start()
    {
        t = transform;

        // Get all SpriteRenderers (including children)
        allRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (allRenderers.Length > 1)
        {
            // Multi-part object (character with children)
            isMultiPart = true;
            originalOrderInLayer = new int[allRenderers.Length];
            for (int i = 0; i < allRenderers.Length; i++)
            {
                originalOrderInLayer[i] = allRenderers[i].sortingOrder;
            }
        }
        else if (allRenderers.Length == 1)
        {
            // Single object (entity/item)
            isMultiPart = false;
        }

        // Calculate sprite bottom for depth sorting
        if (useBottomOfSprite)
        {
            CalculateSpriteBottom();
        }
    }

    private void CalculateSpriteBottom()
    {
        if (allRenderers.Length > 0)
        {
            // Find the lowest point among all renderers
            float lowestPoint = float.MaxValue;

            foreach (SpriteRenderer renderer in allRenderers)
            {
                if (renderer != null && renderer.sprite != null)
                {
                    // Get sprite bounds in world space
                    Bounds bounds = renderer.bounds;
                    float bottom = bounds.min.y;

                    if (bottom < lowestPoint)
                    {
                        lowestPoint = bottom;
                    }
                }
            }

            // Store the offset from transform position to sprite bottom
            if (lowestPoint != float.MaxValue)
            {
                spriteBottom = lowestPoint - transform.position.y;
            }
        }
    }

    private void LateUpdate()
    {
        // Update Z position (works for both single and multi-part)
        Vector3 pos = transform.position;
        pos.z = pos.y * 0.0001f;
        transform.position = pos;

        // Calculate sorting order based on the bottom of the sprite + offset
        float sortingY = useBottomOfSprite ?
            transform.position.y + spriteBottom + yOffset :
            transform.position.y + yOffset;

        if (isMultiPart)
        {
            // Multi-part: Update Order in Layer for all children
            // Invert Y and add offset to avoid negative values
            int baseSortingOrder = Mathf.RoundToInt(-sortingY * 100) + 10000;
            for (int i = 0; i < allRenderers.Length; i++)
            {
                if (allRenderers[i] != null)
                {
                    allRenderers[i].sortingOrder = baseSortingOrder + originalOrderInLayer[i];
                }
            }
        }
        else if (allRenderers.Length == 1)
        {
            // Single part: Update Order in Layer for the single renderer
            // Invert Y and add offset to avoid negative values
            int sortingOrder = Mathf.RoundToInt(-sortingY * 100) + 10000;
            allRenderers[0].sortingOrder = sortingOrder;
        }

        if (stationary)
        {
            Destroy(this);
        }
    }
}