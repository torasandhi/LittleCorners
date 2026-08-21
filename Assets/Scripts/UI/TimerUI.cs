using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text TimerText;

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        TimerText.text = GameManager.Instance.GetFormattedCurrentTime();
    }
}