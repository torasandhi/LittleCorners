using UnityEngine;
using TMPro; 

public class ProgressUI : MonoBehaviour
{
    public TMP_Text progressText;

    private void OnEnable()
    {
        GameManager.Instance.OnProgressUpdated += UpdateUI;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnProgressUpdated -= UpdateUI;
        }
    }

    private void UpdateUI(int current, int total)
    {
        progressText.text = $"{current} / {total}";
    }
}