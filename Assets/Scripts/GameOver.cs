using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private string MainMenuSceneName = "MainMenu";
    
    public void Retry()
    {
        SceneManager.LoadScene("Level 1");    }

    public void MainMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }
}
