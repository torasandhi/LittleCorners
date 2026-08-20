using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Camera cam;

    private Vector3 offset;

    private bool isDragging;

    public string targetZoneTag = "DropZone";
    public string ObjectInfo = "Triangle";
    private Transform validZone;

    [Tooltip("Layer(s) this draggable object lives on. Keeps the raycast from picking up unrelated objects.")]
    public LayerMask draggableLayer = ~0;

    [Tooltip("Max distance the selection raycast will check.")]
    public float raycastMaxDistance = 100f;

    [Header("Wall Collision")]
    [Tooltip("Layer(s) that count as walls/obstacles the object should not be dragged through.")]
    public LayerMask wallLayer;

    [Tooltip("Roughly the object's radius, used to sweep-test movement against walls. Increase if the object still clips corners.")]
    public float collisionRadius = 0.4f;

    [Tooltip("Small buffer kept between the object and a wall it stops against.")]
    public float wallSkinWidth = 0.05f;

    private Renderer objRenderer;
    public Color defaultColor = Color.white;
    public Color glowColor = new Color(1f, 1f, 0.5f, 1f);

    // Virtual horizontal plane the object drags along, built at the object's current height
    private Plane dragPlane;

    private void Awake()
    {
        cam = Camera.main;

        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null) objRenderer.material.color = defaultColor;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
        }
        else if (isDragging && Input.GetMouseButton(0))
        {
            DoDrag();
        }
        else if (isDragging && Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    private void TryStartDrag()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance, draggableLayer))
            return;

        if (hit.transform != transform)
            return;

        isDragging = true;

        // Build the drag plane at the object's current height so it moves on a flat plane
        // as the mouse moves, rather than jumping to whatever surface is under the cursor.
        dragPlane = new Plane(Vector3.up, transform.position);

        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            offset = transform.position - hitPoint;
        }
    }

    private void DoDrag()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!dragPlane.Raycast(ray, out float enter))
            return;

        Vector3 targetPosition = ray.GetPoint(enter) + offset;
        transform.position = GetWallClampedPosition(transform.position, targetPosition);
    }

    // Sweeps a sphere from the current position toward the target position. If a wall is
    // in the way, the object stops just short of it instead of passing through.
    private Vector3 GetWallClampedPosition(Vector3 current, Vector3 target)
    {
        Vector3 delta = target - current;
        float distance = delta.magnitude;

        if (distance < 0.0001f)
            return current;

        Vector3 direction = delta / distance;

        if (Physics.SphereCast(current, collisionRadius, direction, out RaycastHit hit, distance, wallLayer))
        {
            float safeDistance = Mathf.Max(hit.distance - wallSkinWidth, 0f);
            return current + direction * safeDistance;
        }

        return target;
    }

    private void EndDrag()
    {
        isDragging = false;

        bool isValidPlacement = validZone != null;

        if (isValidPlacement)
        {
            transform.position = validZone.position;

            if (objRenderer != null) objRenderer.material.color = defaultColor;

            GameManager.Instance.ItemPlaced();
            Destroy(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetZoneTag))
            return;

        var dropZoneObj = other.GetComponent<DropZone>();
        if (dropZoneObj == null) return;

        if (dropZoneObj.GetZoneInfo() != ObjectInfo) return;

        validZone = other.transform;

        if (objRenderer != null) objRenderer.material.color = glowColor;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetZoneTag))
            return;

        var dropZoneObj = other.GetComponent<DropZone>();
        if (dropZoneObj == null) return;

        if (dropZoneObj.GetZoneInfo() != ObjectInfo) return;

        validZone = null;

        if (objRenderer != null) objRenderer.material.color = defaultColor;
    }
}