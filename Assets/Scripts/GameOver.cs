using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private string MainMenuSceneName = "MainMenu";
    
    public void Retry()
    {
        SceneManager.LoadScene(GameManager.Instance.GetCurrentLevelIndex());
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }
}
