using System.Numerics;
using System.Threading;
using UnityEngine;

public class Grabable : MonoBehaviour
{
    Rigidbody rb;
    private bool isGrabbed = false;
    private Camera mainCamera;
    private float baseMaxLinearVelocity = 10f;
    private float baseMaxAngularVelocity = 10f;

    public float scoreClock = 0f; // Clock for scoring
    private float distanceClock = 0f; // Clock for distance calculation
    public Transform EndPoint;
    public float distanceToEndPoint = 0f;
    public float distanceToEndPointThreshold = 1f; // Threshold for distance to endpoint
    Bossman Boss;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = baseMaxAngularVelocity;
        rb.maxLinearVelocity = baseMaxLinearVelocity;
        mainCamera = Camera.main;
        EndPoint = GameObject.FindWithTag("EndPoint").transform; // Find the endpoint object by tag
        Boss = GameObject.FindWithTag("Bossman").GetComponent<Bossman>(); // Find
    }

    // Update is called once per frame
    void Update()
    {

        #region Dragging
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
                UnityEngine.Vector3 targetPosition = new UnityEngine.Vector3(hit.point.x, 1, hit.point.z);
                UnityEngine.Vector3 direction = targetPosition - transform.position;

                // Adjust velocities based on distance
                float distance = direction.magnitude;
                rb.maxLinearVelocity = baseMaxLinearVelocity + distance * 0.5f;
                rb.maxAngularVelocity = baseMaxAngularVelocity + distance * 0.5f;

                // Move the object towards the target position
                rb.linearVelocity = direction * 10f; // Adjust speed as needed
            }
        }

        // Check if the object is outside the camera's bounds
        UnityEngine.Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        if (viewportPosition.x < 0 || viewportPosition.x > 1 || viewportPosition.y < 0 || viewportPosition.y > 1)
        {
            Debug.Log("Game Over: Object left the camera's bounds!");
        }
    #endregion Dragging
            
    //----------------------------------------------------//
    
    scoreClock += Time.deltaTime;
    }

    #region Scoring
    void OnTriggerStay(Collider other) // Check if the object is within the endpoint's trigger collider
    {
        if(other.tag == "EndPoint")
        {
        distanceClock += Time.deltaTime;

        if (distanceClock >= 0.5f)
        {
            distanceToEndPoint = UnityEngine.Vector3.Distance(transform.position, EndPoint.position);
            // Stop the timer and calculate score
            float score = 10 - ((scoreClock*2f) + (distanceToEndPoint*1.5f));
            Debug.Log("Score: " + score + "\n Time taken, distance to endpoint: " + scoreClock + ", " + distanceToEndPoint);
            Boss.MoveEndPoint(new UnityEngine.Vector3(Random.Range(8,-8), 0.1f,Random.Range(5.5f,-3.5f))); // This should change to be bounds of camera
            Destroy(gameObject);
        }
        }
    }

    void OnTriggerExit(Collider other) //reset distance clock when object leaves endpoint
    {
        if(other.tag == "EndPoint")
        {
            distanceClock = 0f;
        }
    }
    #endregion Scoring
}