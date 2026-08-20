using UnityEngine;

public class DropZone : MonoBehaviour
{
    [SerializeField]private string ZoneInfo = "Furniture";
    
    public Renderer zoneRenderer { get; private set; }

    private void Awake()
    {
        zoneRenderer = GetComponent<Renderer>();
        if (zoneRenderer != null)
        {
            Color c = zoneRenderer.material.color;
            c.a = 0f;
            zoneRenderer.material.color = c;
        }
    }
    
    public string GetZoneInfo()
    {
        return ZoneInfo;
    }
}