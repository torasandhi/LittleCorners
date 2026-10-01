using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int BestScore, CurrentScore;

    private bool isCompletingLevel;

    private int CurrentLevelIndex;
    private int pendingNextSceneIndex = -1;
    private LevelCompleteUI levelCompleteUI;

    public int totalItemsToPlace = 2;
    private int itemsPlaced = 0;

    public string mainMenuSceneName = "MainMenu";

    public event Action<int, int> OnProgressUpdated;

    [HideInInspector]
    public List<DropZone> allDropZones = new List<DropZone>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        levelCompleteUI = GetComponent<LevelCompleteUI>();

        allDropZones.AddRange(
            FindObjectsByType<DropZone>(FindObjectsSortMode.None)
        );

        DontDestroyOnLoad(gameObject);

        CurrentScore = 0;
        LoadBestScore();
    }

    private void Start()
    {
        OnProgressUpdated?.Invoke(itemsPlaced, totalItemsToPlace);
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        levelCompleteUI?.HideImmediately();
        isCompletingLevel = false;
        pendingNextSceneIndex = -1;
        itemsPlaced = 0;
        CurrentLevelIndex = scene.buildIndex;

        allDropZones.Clear();

        allDropZones.AddRange(
            FindObjectsByType<DropZone>(FindObjectsSortMode.None)
        );

        // Each level has one drop zone per item. Recalculate the target from the
        // loaded scene so replaying Level 1 cannot inherit Level 2's item count.
        if (allDropZones.Count > 0)
            totalItemsToPlace = allDropZones.Count;

        OnProgressUpdated?.Invoke(itemsPlaced, totalItemsToPlace);
    }

    public void ItemPlaced()
    {
        if (isCompletingLevel)
            return;

        itemsPlaced++;
        AddCurretScore();

        OnProgressUpdated?.Invoke(itemsPlaced, totalItemsToPlace);

        if (itemsPlaced >= totalItemsToPlace)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        isCompletingLevel = true;

        Scene completedScene = SceneManager.GetActiveScene();
        LevelProgress.MarkCleared(completedScene.buildIndex);

        Debug.Log($"Room Complete! Saved progress for {completedScene.name}.");

        pendingNextSceneIndex = completedScene.buildIndex + 1;

        if (levelCompleteUI != null)
        {
            levelCompleteUI.Show(completedScene.buildIndex);
        }
        else
        {
            ContinueToNextLevel();
        }
    }

    public void ContinueToNextLevel()
    {
        if (!isCompletingLevel)
            return;

        Time.timeScale = 1f;
        levelCompleteUI?.HideImmediately();

        if (pendingNextSceneIndex >= 0 &&
            pendingNextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            CurrentLevelIndex = pendingNextSceneIndex;
            SceneManager.LoadScene(pendingNextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void ReturnToMainMenu()
    {
        if (!isCompletingLevel)
            return;

        Time.timeScale = 1f;
        levelCompleteUI?.HideImmediately();
        ResetScore();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void AddCurretScore()
    {
        CurrentScore++;

        if (CurrentScore < BestScore)
            return;

        PlayerPrefs.SetInt("BestScore", CurrentScore);
        PlayerPrefs.Save();
    }

    private void LoadBestScore()
    {
        BestScore = PlayerPrefs.GetInt("BestScore", 0);
    }

    public int GetBestScore()
    {
        return BestScore;
    }

    public int GetCurretScore()
    {
        return CurrentScore;
    }

    public int GetCurrentLevelIndex()
    {
        return CurrentLevelIndex;
    }

    // =========================
    // PROGRESS
    // =========================

    public void BroadcastProgressUpdate()
    {
        OnProgressUpdated?.Invoke(itemsPlaced, totalItemsToPlace);
    }

    public void ResetScore()
    {
        CurrentScore = 0;   
        Debug.LogWarning("Alamak");
    }
}
