using UnityEngine;
using UnityEngine.SceneManagement; 
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    [Tooltip("Order: Level 1, Level 2, Level 3")]
    [SerializeField] private GameObject[] clearedLevelImages;

    private void Start()
    {
        if (clearedLevelImages == null)
            return;

        for (int index = 0; index < clearedLevelImages.Length; index++)
        {
            GameObject levelImage = clearedLevelImages[index];

            if (levelImage != null)
                levelImage.SetActive(LevelProgress.IsCleared(index + 1));
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettings()
    {
        Debug.Log("Settings opened!");
    }

    public void QuitGame()
    {
        Debug.Log("Game quit!");
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
