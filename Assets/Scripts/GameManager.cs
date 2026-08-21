using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int BestScore, CurrentScore;

    private float CurrentTime;
    private bool isTimerRunning;

    private int CurrentLevelIndex;

    public int totalItemsToPlace = 2;
    private int itemsPlaced = 0;

    public string mainMenuSceneName = "MainMenu";

    public event Action<int, int> OnProgressUpdated;

    [HideInInspector]
    public List<DropZone> allDropZones = new List<DropZone>();

    private const float LevelTime = 60f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        allDropZones.AddRange(
            FindObjectsByType<DropZone>(FindObjectsSortMode.None)
        );

        DontDestroyOnLoad(gameObject);

        CurrentScore = 0;
        CurrentTime = LevelTime;

        LoadBestScore();
    }

    private void Start()
    {
        StartLevelTimer();

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
        allDropZones.Clear();

        allDropZones.AddRange(
            FindObjectsByType<DropZone>(FindObjectsSortMode.None)
        );
    }

    private void Update()
    {
        if (!isTimerRunning)
            return;

        CurrentTime -= Time.deltaTime;

        if (CurrentTime <= 0f)
        {
            CurrentTime = 0f;
            isTimerRunning = false;

            Debug.Log("Time's Up! Game Over.");

            SceneManager.LoadScene("GameOver");
        }
    }
    
    public void ItemPlaced()
    {
        itemsPlaced++;
        AddCurretScore();

        OnProgressUpdated?.Invoke(itemsPlaced, totalItemsToPlace);

        if (itemsPlaced >= totalItemsToPlace)
        {
            Debug.Log("Room Complete! Loading Next Chapter...");

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

            totalItemsToPlace += 2;
            itemsPlaced = 0;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                CurrentLevelIndex = nextSceneIndex;

                // Reset timer for the next level
                CurrentTime = LevelTime;
                isTimerRunning = true;

                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                StopTimer();
                SceneManager.LoadScene(0);
            }
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
    // TIMER
    // =========================

    public float GetCurrentTime()
    {
        return CurrentTime;
    }

    public string GetFormattedCurrentTime()
    {
        int minutes = Mathf.FloorToInt(CurrentTime / 60f);
        int seconds = Mathf.FloorToInt(CurrentTime % 60f);

        return $"{minutes:00}:{seconds:00}";
    }

    public void StartLevelTimer()
    {
        CurrentTime = LevelTime;
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    // =========================
    // PROGRESS
    // =========================

    public void BroadcastProgressUpdate()
    {
        OnProgressUpdated?.Invoke(itemsPlaced, totalItemsToPlace);
    }
}