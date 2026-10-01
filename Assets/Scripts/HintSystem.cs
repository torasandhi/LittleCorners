using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class HintSystem : MonoBehaviour
{
    public Button hintButton;
    public float cooldownTime = 30f;
    
    private bool isCooldown = false;
    private DraggableObject currentSelectedItem;
    private TMP_Text hintButtonText;

    private void OnEnable()
    {
        EnsureEventSystem();
        DraggableObject.OnItemSelected += HandleItemSelected;
        hintButton.onClick.AddListener(UseHint);
        hintButtonText = hintButton.GetComponentInChildren<TMP_Text>();
        hintButton.gameObject.SetActive(true);
        UpdateButtonState();
    }

    private void OnDisable()
    {
        DraggableObject.OnItemSelected -= HandleItemSelected;
        hintButton.onClick.RemoveListener(UseHint);
    }

    private void HandleItemSelected(DraggableObject item)
    {
        currentSelectedItem = item;
        UpdateButtonState();
    }

    private void UseHint()
    {
        if (currentSelectedItem == null || isCooldown) return;

        foreach (var zone in GameManager.Instance.allDropZones)
        {
            if (zone.GetZoneInfo() == currentSelectedItem.ObjectInfo)
            {
                // CHANGED: Pass the DropZone component directly instead of its transform
                StartCoroutine(HighlightZone(zone));
                StartCoroutine(HintCooldown());
                break;
            }
        }
    }

    private IEnumerator HighlightZone(DropZone zone)
    {
        if (zone.zoneRenderer != null)
        {
            Color color = zone.zoneRenderer.material.color;
            
            color.a = 0.5f; 
            zone.zoneRenderer.material.color = color; 
            
            yield return new WaitForSeconds(2f);
            
            color.a = 0f; 
            zone.zoneRenderer.material.color = color;
        }
    }

    private IEnumerator HintCooldown()
    {
        isCooldown = true;
        hintButton.interactable = false;

        float timeRemaining = cooldownTime;
        while (timeRemaining > 0f)
        {
            if (hintButtonText != null)
                hintButtonText.text = $"HINT {Mathf.CeilToInt(timeRemaining)}";

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        isCooldown = false;
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        hintButton.gameObject.SetActive(true);
        hintButton.interactable = currentSelectedItem != null && !isCooldown;

        if (!isCooldown && hintButtonText != null)
            hintButtonText.text = "HINT";
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        new GameObject(
            "GameplayEventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));
    }
}
