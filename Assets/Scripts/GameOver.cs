using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private string MainMenuSceneName = "MainMenu";
    
    public void Retry()
    {
        GameManager.Instance.totalItemsToPlace = 2;
        SceneManager.LoadScene("Level 1");    }

    public void MainMenu()
    {
        GameManager.Instance.totalItemsToPlace = 2;
        SceneManager.LoadScene(MainMenuSceneName);
    }
}
