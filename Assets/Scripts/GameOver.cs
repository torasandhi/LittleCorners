using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void Retry()
    {
        SceneManager.LoadScene(GameManager.Instance.GetCurrentLevelIndex());
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
