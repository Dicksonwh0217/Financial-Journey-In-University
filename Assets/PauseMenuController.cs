using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PauseMenuController : MonoBehaviour
{
    [SerializeField] GameObject PauseMenuPanel;
    [SerializeField] GameObject[] otherUIPanels; // Drag other UI panels here

    private bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    private void HandleEscapeKey()
    {
        // First check if any other UI panels are open and close the most recent one
        if (HasOpenUIPanel())
        {
            return;
        }
        // If no other UI is open, toggle pause menu
        else
        {
            TogglePauseMenu();
        }
    }

    private bool HasOpenUIPanel()
    {
        foreach (GameObject panel in otherUIPanels)
        {
            if (panel != null && panel.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        PauseMenuPanel.SetActive(isPaused);

        // Pause/unpause the game
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void OpenPauseMenu()
    {
        isPaused = true;
        PauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ClosePauseMenu()
    {
        isPaused = false;
        PauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Call this method from other UI controllers when they open
    public void RegisterOpenUI(GameObject uiPanel)
    {
        // You could implement a stack-based system here for better UI management
    }
}