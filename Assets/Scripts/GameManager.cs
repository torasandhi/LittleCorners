using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int BestScore, CurrentScore;

    private int CurrentLevelIndex;

    public int totalItemsToPlace = 2;
    private int itemsPlaced = 0;

    public string mainMenuSceneName = "MainMenu";

    public event Action<int, int> OnProgressUpdated;

    [HideInInspector] public List<DropZone> allDropZones = new List<DropZone>();

    private void Awake()
    {
        if (Instance == null) Instance = this;

        allDropZones.AddRange(FindObjectsByType<DropZone>(FindObjectsSortMode.None));
        CurrentScore = 0;
        LoadBestScore();
    }

    private void Start()
    {
        OnProgressUpdated?.Invoke(itemsPlaced, totalItemsToPlace);
    }

    public void ItemPlaced()
    {
        itemsPlaced++;

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
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    public void AddCurretScore()
    {
        CurrentScore++;
        if (CurrentScore < BestScore) return;
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
}