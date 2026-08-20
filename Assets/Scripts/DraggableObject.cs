using System;
using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private Camera cam;
    private Rigidbody rb;
    private bool isSelected;
    private bool isDragging;
    private bool hasBeenPlaced;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Vector3 offset;

    public string targetZoneTag = "DropZone";
    public string ObjectInfo = "Triangle";
    private Transform validZone;

    public LayerMask draggableLayer = ~0;
    public LayerMask environmentLayer = ~0;

    public float raycastMaxDistance = 100f;
    public float liftHeight = 1f;
    public float dragSpeed = 15f;
    public float snapDistance = 1.5f;

    private Renderer objRenderer;
    public Color defaultColor = Color.white;
    public Color glowColor = new Color(1f, 1f, 0.5f, 1f);
    public Color selectedColor = new Color(0.8f, 0.8f, 1f, 1f);

    public static event Action<DraggableObject> OnItemSelected;

    // Changed: Added lifecycle methods to listen for when other objects are selected
    private void OnEnable()
    {
        OnItemSelected += HandleNewSelection;
    }

    // Changed: Remove listener when disabled
    private void OnDisable()
    {
        OnItemSelected -= HandleNewSelection;
    }

    // Changed: Automatically deselect this object if the player selects a different one
    private void HandleNewSelection(DraggableObject newlySelected)
    {
        if (newlySelected != this && isSelected)
        {
            CancelSelection(false);
        }
    }

    private void Awake()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null) objRenderer.material.color = defaultColor;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && isSelected)
        {
            // Changed: Explicitly tell the method to broadcast the null selection to the UI
            CancelSelection(true);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TrySelect();
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateTargetPosition();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            TryDrop();
        }
    }

    private void FixedUpdate()
    {
        if (isDragging && rb != null)
        {
            Vector3 newPos = Vector3.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * dragSpeed);
            rb.MovePosition(newPos);

            if (validZone != null)
            {
                rb.MoveRotation(Quaternion.Lerp(rb.rotation, validZone.rotation, Time.fixedDeltaTime * dragSpeed));

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
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

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
        }

        if (Physics.Raycast(ray, out RaycastHit envHit, raycastMaxDistance, environmentLayer))
        {
            offset = transform.position - envHit.point;
        }
        else
        {
            offset = Vector3.zero;
        }

        targetPosition = transform.position + (Vector3.up * liftHeight);

        if (objRenderer != null) objRenderer.material.color = selectedColor;
        OnItemSelected?.Invoke(this);
    }

    private void UpdateTargetPosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance, environmentLayer))
        {
            targetPosition = hit.point + offset + (Vector3.up * liftHeight);

            if (validZone != null)
            {
                Vector3 hitXZ = new Vector3(hit.point.x, 0f, hit.point.z);
                Vector3 zoneXZ = new Vector3(validZone.position.x, 0f, validZone.position.z);

                if (Vector3.Distance(hitXZ, zoneXZ) <= snapDistance)
                {
                    targetPosition = validZone.position + (Vector3.up * liftHeight);
                }
            }
        }
    }

    private void TryDrop()
    {
        isDragging = false;
        bool isValidPlacement = validZone != null;

        if (isValidPlacement)
        {
            // CHANGED: Properly deselect the item after a successful drop
            isSelected = false;

            transform.position = validZone.position;
            transform.rotation = validZone.rotation;

            if (objRenderer != null) objRenderer.material.color = defaultColor;

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            if (!hasBeenPlaced)
            {
                GameManager.Instance.ItemPlaced();
                hasBeenPlaced = true;
            }

            OnItemSelected?.Invoke(null);
        }
        else
        {
            if (objRenderer != null) objRenderer.material.color = selectedColor;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
    }

    // Changed: Added a 'broadcast' parameter so we don't accidentally hide the UI when switching items
    private void CancelSelection(bool broadcast = true)
    {
        isSelected = false;
        isDragging = false;
        transform.position = originalPosition;

        if (objRenderer != null) objRenderer.material.color = defaultColor;

        // Changed: Only broadcast if requested
        if (broadcast)
        {
            OnItemSelected?.Invoke(null);
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
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