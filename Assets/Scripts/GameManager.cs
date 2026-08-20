using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int totalItemsToPlace;
    private int itemsPlaced = 0;

    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ItemPlaced()
    {
        itemsPlaced++;

        if (itemsPlaced >= totalItemsToPlace)
        {
            Debug.Log("Room Complete! Loading Next Chapter...");
            
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(0);
            }
            
        }
    }
}