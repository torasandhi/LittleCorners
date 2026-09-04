using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject PausePanelUI, PauseButtonUI;
    [SerializeField] private KeyCode PauseKey = KeyCode.Escape;
    [SerializeField] private string MainMenuSceneName = "MainMenu";

    public static bool IsGamePaused { get; private set; } = false;

    private void Start()
    {
        ResumeGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(PauseKey))
        {
            if (IsGamePaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        PausePanelUI.SetActive(true);
        PauseButtonUI.SetActive(false);
        Time.timeScale = 0f;
        IsGamePaused = true;
    }

    public void ResumeGame()
    {
        PausePanelUI.SetActive(false);
        PauseButtonUI.SetActive(true);
        Time.timeScale = 1f; 
        IsGamePaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        IsGamePaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        IsGamePaused = false;
        GameManager.Instance.ResetScore();
        SceneManager.LoadScene(MainMenuSceneName);
    }
}