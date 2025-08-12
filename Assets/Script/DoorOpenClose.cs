using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpenClose : MonoBehaviour
{
    [Header("Door GameObjects")]
    [SerializeField] GameObject openDoor;
    [SerializeField] GameObject closeDoor;

    [Header("Audio Clips")]
    [SerializeField] AudioClip onOpenAudio;
    [SerializeField] AudioClip onCloseAudio;

    [Header("Audio Settings")]
    [SerializeField][Range(0f, 1f)] float openAudioVolume = 1f;
    [SerializeField][Range(0f, 1f)] float closeAudioVolume = 1f;
    [SerializeField][Range(0.1f, 3f)] float openAudioPitch = 1f;
    [SerializeField][Range(0.1f, 3f)] float closeAudioPitch = 1f;
    [SerializeField] bool randomizePitch = false;
    [SerializeField][Range(0f, 0.5f)] float pitchVariation = 0.1f;

    [Header("Audio Delay (Optional)")]
    [SerializeField] float openAudioDelay = 0f;
    [SerializeField] float closeAudioDelay = 0f;

    [Header("Audio Overlap Prevention")]
    [SerializeField] bool preventAudioOverlap = true;

    // Private variables to track audio state
    private bool isAudioPlaying = false;

    private void Start()
    {
        // Set initial sorting orders to match parent
        InheritParentSortingOrder();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Character>() != null)
        {
            OpenDoor();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Character>() != null)
        {
            CloseDoor();
        }
    }

    private void CloseDoor()
    {
        closeDoor.SetActive(true);
        openDoor.SetActive(false);
        InheritParentSortingOrder(); // Inherit parent's order

        if (onCloseAudio != null && ShouldPlayAudio())
        {
            PlayCloseAudio();
        }
    }

    private void OpenDoor()
    {
        openDoor.SetActive(true);
        closeDoor.SetActive(false);
        InheritParentSortingOrder(); // Inherit parent's order

        if (onOpenAudio != null && ShouldPlayAudio())
        {
            PlayOpenAudio();
        }
    }

    private void InheritParentSortingOrder()
    {
        // Get the parent's SpriteRenderer (bakery)
        SpriteRenderer parentRenderer = GetComponentInParent<SpriteRenderer>();

        if (parentRenderer != null)
        {
            int parentSortingOrder = parentRenderer.sortingOrder;
            string parentSortingLayer = parentRenderer.sortingLayerName;

            // Apply same sorting order and layer to open door
            if (openDoor != null)
            {
                SpriteRenderer[] openRenderers = openDoor.GetComponentsInChildren<SpriteRenderer>();
                foreach (var renderer in openRenderers)
                {
                    renderer.sortingOrder = parentSortingOrder;
                    renderer.sortingLayerName = parentSortingLayer;
                }
            }

            // Apply same sorting order and layer to close door
            if (closeDoor != null)
            {
                SpriteRenderer[] closeRenderers = closeDoor.GetComponentsInChildren<SpriteRenderer>();
                foreach (var renderer in closeRenderers)
                {
                    renderer.sortingOrder = parentSortingOrder;
                    renderer.sortingLayerName = parentSortingLayer;
                }
            }
        }
    }

    private void PlayOpenAudio()
    {
        if (openAudioDelay > 0f)
        {
            StartCoroutine(PlayAudioWithDelay(() => PlayAudioWithSettings(onOpenAudio, openAudioVolume, openAudioPitch), openAudioDelay));
        }
        else
        {
            PlayAudioWithSettings(onOpenAudio, openAudioVolume, openAudioPitch);
        }
    }

    private void PlayCloseAudio()
    {
        if (closeAudioDelay > 0f)
        {
            StartCoroutine(PlayAudioWithDelay(() => PlayAudioWithSettings(onCloseAudio, closeAudioVolume, closeAudioPitch), closeAudioDelay));
        }
        else
        {
            PlayAudioWithSettings(onCloseAudio, closeAudioVolume, closeAudioPitch);
        }
    }

    private void PlayAudioWithSettings(AudioClip clip, float volume, float pitch)
    {
        if (clip == null) return;

        isAudioPlaying = true;

        if (randomizePitch && pitchVariation > 0f)
        {
            float finalPitch = pitch + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            AudioManager.instance.Play(clip, volume, finalPitch);
        }
        else
        {
            AudioManager.instance.Play(clip, volume, pitch);
        }

        // Start coroutine to track when audio finishes
        StartCoroutine(WaitForAudioToFinish(clip.length));
    }

    private IEnumerator WaitForAudioToFinish(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        OnAudioComplete();
    }

    private IEnumerator PlayAudioWithDelay(System.Action audioAction, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioAction?.Invoke();
    }

    private bool ShouldPlayAudio()
    {
        // If overlap prevention is disabled, always play
        if (!preventAudioOverlap) return true;

        // Only play if no audio is currently playing
        return !isAudioPlaying;
    }

    private void OnAudioComplete()
    {
        isAudioPlaying = false;
    }
}