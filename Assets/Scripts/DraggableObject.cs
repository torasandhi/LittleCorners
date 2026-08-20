using System;
using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private Camera cam;
    private Vector3 offset;
    
    private bool isDragging;
    private bool isSelected; 

    public string targetZoneTag = "DropZone";
    public string ObjectInfo = "Triangle";
    private Transform validZone;

    public LayerMask draggableLayer = ~0;
    public float raycastMaxDistance = 100f;

    [Header("Wall Collision")]
    public LayerMask wallLayer;
    public float collisionRadius = 0.4f;
    public float wallSkinWidth = 0.05f;

    private Renderer objRenderer;
    public Color defaultColor = Color.white;
    public Color glowColor = new Color(1f, 1f, 0.5f, 1f);
    
    public Color selectedColor = new Color(0.8f, 0.8f, 1f, 1f);

    private Plane dragPlane;
    public static event Action<DraggableObject> OnItemSelected;
    private Vector3 originalPosition;

    private void Awake()
    {
        cam = Camera.main;
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null) objRenderer.material.color = defaultColor;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && isSelected)
        {
            CancelSelection();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TrySelect();
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            DoDrag();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            TryDrop();
        }
    }

    private void TrySelect()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance, draggableLayer)) return;
        if (hit.transform != transform) return;

        isSelected = true;
        isDragging = true;
        originalPosition = transform.position;

        dragPlane = new Plane(Vector3.up, transform.position);

        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            offset = transform.position - hitPoint;
        }

        if (objRenderer != null) objRenderer.material.color = selectedColor;
        OnItemSelected?.Invoke(this);
    }

    private void DoDrag()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!dragPlane.Raycast(ray, out float enter))
            return;

        Vector3 targetPosition = ray.GetPoint(enter) + offset;
        transform.position = GetWallClampedPosition(transform.position, targetPosition);
    }

    private Vector3 GetWallClampedPosition(Vector3 current, Vector3 target)
    {
        Vector3 delta = target - current;
        float distance = delta.magnitude;

        if (distance < 0.0001f) return current;

        Vector3 direction = delta / distance;

        if (Physics.SphereCast(current, collisionRadius, direction, out RaycastHit hit, distance, wallLayer))
        {
            float safeDistance = Mathf.Max(hit.distance - wallSkinWidth, 0f);
            return current + direction * safeDistance;
        }

        return target;
    }

    private void TryDrop()
    {
        isDragging = false;
        bool isValidPlacement = validZone != null;

        if (isValidPlacement)
        {
            transform.position = validZone.position;
            if (objRenderer != null) objRenderer.material.color = defaultColor;

            GameManager.Instance.ItemPlaced();
            OnItemSelected?.Invoke(null);
            Destroy(this);
        }
        else
        {
            transform.position = originalPosition;
            if (objRenderer != null) objRenderer.material.color = selectedColor; 
        }
    }

    private void CancelSelection()
    {
        isSelected = false;
        isDragging = false;
        transform.position = originalPosition;
        
        if (objRenderer != null) objRenderer.material.color = defaultColor;
        OnItemSelected?.Invoke(null);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetZoneTag)) return;

        var dropZoneObj = other.GetComponent<DropZone>();
        if (dropZoneObj == null) return;
        if (dropZoneObj.GetZoneInfo() != ObjectInfo) return;

        validZone = other.transform;
        if (objRenderer != null) objRenderer.material.color = glowColor;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetZoneTag)) return;

        var dropZoneObj = other.GetComponent<DropZone>();
        if (dropZoneObj == null) return;
        if (dropZoneObj.GetZoneInfo() != ObjectInfo) return;

        validZone = null;
        
        if (objRenderer != null) objRenderer.material.color = isSelected ? selectedColor : defaultColor;
    }
}