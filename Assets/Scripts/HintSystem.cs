using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HintSystem : MonoBehaviour
{
    public Button hintButton;
    public float cooldownTime = 30f;
    
    private bool isCooldown = false;
    private DraggableObject currentSelectedItem;

    private void OnEnable()
    {
        DraggableObject.OnItemSelected += HandleItemSelected;
        hintButton.onClick.AddListener(UseHint);
        hintButton.gameObject.SetActive(false); 
    }

    private void OnDisable()
    {
        DraggableObject.OnItemSelected -= HandleItemSelected;
        hintButton.onClick.RemoveListener(UseHint);
    }

    private void HandleItemSelected(DraggableObject item)
    {
        currentSelectedItem = item;
        hintButton.gameObject.SetActive(item != null && !isCooldown);
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
        hintButton.gameObject.SetActive(false);
        
        yield return new WaitForSeconds(cooldownTime);
        
        isCooldown = false;
        hintButton.interactable = true;
        
        if (currentSelectedItem != null) 
        {
            hintButton.gameObject.SetActive(true);
        }
    }
}