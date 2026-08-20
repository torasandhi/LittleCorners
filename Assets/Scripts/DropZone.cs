using UnityEngine;


public class DropZone : MonoBehaviour
{

    [SerializeField]private string ZoneInfo = "Furniture";
    
    public string GetZoneInfo()
    {
        return ZoneInfo;
    }
}