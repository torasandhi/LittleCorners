using System;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private Text CurrentScoreText;
    [SerializeField] private Text BestScoreText;

    private void Start()
    {
        if (CurrentScoreText == null || BestScoreText == null) return;

        CurrentScoreText.text = "CurrentScore: " + GameManager.Instance.GetCurretScore();
        BestScoreText.text = "BestScore: " + GameManager.Instance.GetBestScore();
    }
}