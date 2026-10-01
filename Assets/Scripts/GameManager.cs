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
        isCompletingLevel = false;
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

        int nextSceneIndex = completedScene.buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            CurrentLevelIndex = nextSceneIndex;
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
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
