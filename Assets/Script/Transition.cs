using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TransitionType
{
    Warp,
    Scene,
    BigMap  // New transition type for going to big map
}

public class Transition : MonoBehaviour
{
    [SerializeField] TransitionType transitionType;
    [SerializeField] string sceneNameToTransition;
    [SerializeField] Vector3 targetPosition;
    [SerializeField] Collider2D confiner;

    [Header("Big Map Settings")]
    [SerializeField] string bigMapSceneName = "BigMap";
    [SerializeField] Vector3 bigMapSpawnPosition = Vector3.zero;

    CameraConfiner cameraConfiner;
    [SerializeField] Transform destination;

    void Start()
    {
        if (confiner != null)
        {
            cameraConfiner = FindFirstObjectByType<CameraConfiner>();
        }

        if (transform.childCount > 1)
            destination = transform.GetChild(1);
    }

    internal void InitiateTransition(Transform toTransition)
    {
        // Add null check for the transform
        if (toTransition == null)
        {
            Debug.LogError("Transform to transition is null!");
            return;
        }

        // Save current scene info before transitioning (except for warp)
        if (transitionType != TransitionType.Warp)
        {
            SaveCurrentSceneInfo();
        }

        switch (transitionType)
        {
            case TransitionType.Warp:
                HandleWarpTransition(toTransition);
                break;

            case TransitionType.Scene:
                HandleSceneTransition();
                break;

            case TransitionType.BigMap:
                HandleBigMapTransition();
                break;
        }
    }

    private void SaveCurrentSceneInfo()
    {
        // Save current scene and player position for returning from BigMap
        string currentScene = SceneManager.GetActiveScene().name;

        // Save to PlayerPrefs (or use a more sophisticated save system)
        PlayerPrefs.SetString("LastScene", currentScene);

        // If there's a player, save their position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 playerPos = player.transform.position;
            PlayerPrefs.SetFloat("LastPosX", playerPos.x);
            PlayerPrefs.SetFloat("LastPosY", playerPos.y);
            PlayerPrefs.SetFloat("LastPosZ", playerPos.z);
        }

        PlayerPrefs.Save();
    }

    public void HandleWarpTransition(Transform toTransition)
    {
        // Add null checks
        if (toTransition == null)
        {
            Debug.LogError("Transform to transition is null in HandleWarpTransition!");
            return;
        }

        if (destination == null)
        {
            Debug.LogError("Destination is null in HandleWarpTransition!");
            return;
        }

        // Updated camera handling for Cinemachine 3.1.4
        HandleCameraWarpForTransition();

        // Update camera confiner if available
        if (cameraConfiner != null && confiner != null)
        {
            cameraConfiner.UpdateBounds(confiner);
        }

        // Move the transform to destination
        toTransition.position = new Vector3(
            destination.position.x,
            destination.position.y,
            toTransition.position.z
        );
    }

    private void HandleCameraWarpForTransition()
    {
        if (destination == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 oldPosition = player.transform.position;

            // Move player to destination
            player.transform.position = destination.position;

            // Use CinemachineCore to notify all cameras of the warp
            Vector3 deltaPosition = oldPosition - destination.position;
            CinemachineCore.OnTargetObjectWarped(player.transform, deltaPosition);
        }
    }

    public void HandleSceneTransition()
    {
        // Add null check for GameSceneManager
        if (GameSceneManager.instance == null)
        {
            Debug.LogError("GameSceneManager instance is null!");
            return;
        }

        // Notify UI Manager that we're leaving BigMap (if we were in it)
        if (UIManager.instance != null && UIManager.instance.IsInBigMap())
        {
            UIManager.instance.SetBigMapMode(false);
        }

        GameSceneManager.instance.InitSwitchScene(sceneNameToTransition, targetPosition);
    }

    private void HandleBigMapTransition()
    {
        // Special handling for big map transition
        if (string.IsNullOrEmpty(bigMapSceneName))
        {
            Debug.LogError("Big Map scene name is not set!");
            return;
        }

        // Add null check for GameSceneManager
        if (GameSceneManager.instance == null)
        {
            Debug.LogError("GameSceneManager instance is null!");
            return;
        }

        StartCoroutine(HandleBigMapTransitionCoroutine());
    }

    // Method to set big map scene from inspector or code
    public void SetBigMapScene(string sceneName, Vector3 spawnPos = default)
    {
        bigMapSceneName = sceneName;
        bigMapSpawnPosition = spawnPos;
    }

    // Method to transition to BigMap from code
    public void TransitionToBigMap()
    {
        transitionType = TransitionType.BigMap;

        // Find player if not provided
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            InitiateTransition(player.transform);
        }
        else
        {
            // If no player found, just switch scene
            HandleBigMapTransition();
        }
    }

    private void OnDrawGizmos()
    {
        switch (transitionType)
        {
            case TransitionType.Scene:
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(sceneNameToTransition))
                {
                    Handles.Label(transform.position, "to " + sceneNameToTransition);
                }
#endif
                break;

            case TransitionType.Warp:
                if (destination != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, destination.position);
                }
                break;

            case TransitionType.BigMap:
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(bigMapSceneName))
                {
                    Handles.Label(transform.position, "to " + bigMapSceneName);
                }
#endif
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
                break;
        }
    }

    private IEnumerator HandleBigMapTransitionCoroutine()
    {
        if (GameSceneManager.instance == null)
        {
            Debug.LogError("GameSceneManager instance is null in HandleBigMapTransitionCoroutine!");
            yield break;
        }

        GameSceneManager.instance.InitSwitchScene(bigMapSceneName, bigMapSpawnPosition);

        // Wait for transition with null checks
        if (GameSceneManager.instance.screenTint != null)
        {
            float duration = 1f / GameSceneManager.instance.screenTint.speed + 0.1f;
            yield return new WaitForSeconds(duration);
        }
        else
        {
            yield return new WaitForSeconds(1f); // Default wait time
        }

        // Hide UI elements with null checks
        GameObject toolBar = GameObject.Find("ToolBar");
        if (toolBar != null)
            toolBar.SetActive(false);

        GameObject statusPanel = GameObject.Find("CharacterStatusPanel");
        if (statusPanel != null)
            statusPanel.SetActive(false);
    }

    public void TransitionToInsideBlockN()
    {
        transitionType = TransitionType.Scene;
        sceneNameToTransition = "BlockNBuilding";
        targetPosition = new Vector3(0.05f, -1.5f, 0);

        // Find player and initiate transition
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            InitiateTransition(player.transform);
        }
        else
        {
            Debug.LogWarning("Player not found, switching scene without player transition");
            HandleSceneTransition();
        }
    }
}