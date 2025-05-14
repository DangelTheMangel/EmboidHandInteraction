using System.Numerics;
using System.Threading;
using UnityEngine;

public class Grabable : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField]private bool isGrabbed = false;
    private Camera mainCamera;
    private float baseMaxLinearVelocity = 10f;
    private float baseMaxAngularVelocity = 10f;

    public float scoreClock = 0f;
    private float distanceClock = 0f; 
    public Transform EndPoint;
    public float distanceToEndPoint = 0f;
    public float distanceToEndPointThreshold = 1f;
    Bossman Boss;
    GameManager gameManager;
    [SerializeField] private bool HandTracking = false;
    
    /// <summary>
    /// Initializes the Rigidbody and sets its maximum linear and angular velocities.
    /// It also finds the main camera, endpoint, and bossman components.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = baseMaxAngularVelocity;
        rb.maxLinearVelocity = baseMaxLinearVelocity;
        mainCamera = Camera.main;
        EndPoint = GameObject.FindWithTag("EndPoint").transform;
        Boss = GameObject.FindWithTag("Bossman").GetComponent<Bossman>(); 
        gameManager = FindAnyObjectByType<GameManager>();

    }
    [SerializeField] HandPostionTransformer handPostion;
    /// <summary>
    /// Sets the grabbed state of the object and assigns a hand position transformer.
    /// This method is used for hand tracking.
    /// </summary>
    /// <param name="grabbed"></param>
    /// <param name="handPostionTransformer"></param>
    public void SetGrabbed(bool grabbed , HandPostionTransformer handPostionTransformer)
    {
        HandTracking = true;
        isGrabbed = grabbed;
        handPostion = handPostionTransformer;
        Debug.Log("Hand set to: " + handPostion);
    }

    /// <summary>
    /// Updates the object's position based on user input.
    /// If the object is grabbed, it moves towards the mouse position or hand position.
    /// </summary>
    void Update()
    {

        #region Dragging
        if(!HandTracking){
            if (Input.GetMouseButtonDown(0))
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

            if (Input.GetMouseButtonUp(0))
            {
                isGrabbed = false;
            }
        }

        if (isGrabbed)
        {
            if(handPostion != null)
            {
                Debug.Log("Hand Position_: " + handPostion.getHandPosition());
                UnityEngine.Vector3 targetPosition = handPostion.getHandPosition();
                UnityEngine.Vector3 direction = targetPosition - transform.position;
                
                float distance = direction.magnitude;
                rb.maxLinearVelocity = baseMaxLinearVelocity + distance * 0.5f;
                rb.maxAngularVelocity = baseMaxAngularVelocity + distance * 0.5f;
                rb.linearVelocity = direction * 10f;
            }
            else
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Debug.Log("Hit: " + hit.collider.gameObject.name);
                    UnityEngine.Vector3 targetPosition = new UnityEngine.Vector3(hit.point.x, 1, hit.point.z);
                    UnityEngine.Vector3 direction = targetPosition - transform.position;
                    float distance = direction.magnitude;
                    rb.maxLinearVelocity = baseMaxLinearVelocity + distance * 0.5f;
                    rb.maxAngularVelocity = baseMaxAngularVelocity + distance * 0.5f;
                    rb.linearVelocity = direction * 10f;
                }
            }
            
        }

        UnityEngine.Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        if (viewportPosition.x < 0 || viewportPosition.x > 1 || viewportPosition.y < 0 || viewportPosition.y > 1)
        {
            Debug.Log("Game Over: Object left the camera's bounds!");
        }
    #endregion Dragging
    
    scoreClock += Time.deltaTime;
    }

    #region Scoring

    /// <summary>
    /// Calculates the score based on the time taken and distance to the endpoint.
    /// It also moves the endpoint to a new random position and destroys the object.
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {
        if(other.tag == "EndPoint")
        {
        distanceClock += Time.deltaTime;

        if (distanceClock >= 0.5f)
        {
            distanceToEndPoint = UnityEngine.Vector3.Distance(transform.position, EndPoint.position);
            float score = 10 - ((scoreClock*2f) + (distanceToEndPoint*1.5f));
            Debug.Log("Score: " + score + "\n Time taken, distance to endpoint: " + scoreClock + ", " + distanceToEndPoint);
            gameManager.EndOfRound(score,scoreClock, distanceToEndPoint); 
            Boss.MoveEndPoint(new UnityEngine.Vector3(Random.Range(8,-8), 0.1f,Random.Range(5.5f,-3.5f))); // This should change to be bounds of camera
            Destroy(gameObject);
        }
        }
    }

    /// <summary>
    /// Resets the distance clock when the object exits the trigger collider of the endpoint.
    /// This is used to prevent scoring when the object is not in the endpoint area.
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        if(other.tag == "EndPoint")
        {
            distanceClock = 0f;
        }
    }
    #endregion Scoring
}