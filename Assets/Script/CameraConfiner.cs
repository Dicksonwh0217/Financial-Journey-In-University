using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraConfiner : MonoBehaviour
{
    [SerializeField] CinemachineConfiner2D confiner;

    // Start is called before the first frame update
    void Start()
    {
        UpdateBounds();
    }

    void OnEnable()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update bounds when a new scene is loaded
        StartCoroutine(UpdateBoundsNextFrame());
    }

    private IEnumerator UpdateBoundsNextFrame()
    {
        // Wait one frame to ensure all objects are initialized
        yield return null;
        UpdateBounds();
    }

    public void UpdateBounds()
    {
        GameObject go = GameObject.Find("CameraConfiner");
        if (go == null)
        {
            confiner.BoundingShape2D = null;
            return;
        }
        Collider2D bounds = go.GetComponent<Collider2D>();
        confiner.BoundingShape2D = bounds;
    }

    internal void UpdateBounds(Collider2D confinerCollider)
    {
        this.confiner.BoundingShape2D = confinerCollider;
    }
}