using UnityEngine;

public class AbyssTrigger : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<DraggableObject>(out var draggable))
        {
            draggable.transform.position = respawnPoint.position;
            
            if (other.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}