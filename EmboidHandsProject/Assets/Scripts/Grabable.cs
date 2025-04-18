using UnityEngine;

public class Grabable : MonoBehaviour
{
    Rigidbody rb;
    private bool isGrabbed = false;
    private Camera mainCamera;
    private float baseMaxLinearVelocity = 10f;
    private float baseMaxAngularVelocity = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = baseMaxAngularVelocity;
        rb.maxLinearVelocity = baseMaxLinearVelocity;
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    isGrabbed = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            isGrabbed = false;
        }

        if (isGrabbed)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetPosition = hit.point;
                Vector3 direction = targetPosition - transform.position;

                // Adjust velocities based on distance
                float distance = direction.magnitude;
                rb.maxLinearVelocity = baseMaxLinearVelocity + distance * 0.5f;
                rb.maxAngularVelocity = baseMaxAngularVelocity + distance * 0.5f;

                // Move the object towards the target position
                rb.linearVelocity = direction * 10f; // Adjust speed as needed
            }
        }

        // Check if the object is outside the camera's bounds
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        if (viewportPosition.x < 0 || viewportPosition.x > 1 || viewportPosition.y < 0 || viewportPosition.y > 1)
        {
            Debug.Log("Game Over: Object left the camera's bounds!");
        }
    }
}