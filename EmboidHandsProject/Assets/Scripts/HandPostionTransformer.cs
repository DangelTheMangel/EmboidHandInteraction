using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using System.Linq;
using UnityEditor;
using UnityEngine.EventSystems;
using Mediapipe.Unity;

public class HandPostionTransformer : MonoBehaviour
{
 // General Settings
    [Header("General Settings")]
    [Tooltip("Layer mask for objects that can be grabbed.")]
    [SerializeField]
    LayerMask layerMask_grabable;

    [Tooltip("Reference to the HandLandmarkerRunner for hand tracking.")]
    [SerializeField]
    HandLandmarkerRunner handLandmarkerRunner;

    [Tooltip("Annotation for visualizing hand landmarks.")]
    [SerializeField]
    MultiHandLandmarkListAnnotation multiHandLandmarkListAnnotation;

    [Tooltip("Prefab for individual hand parts.")]
    [SerializeField]
    GameObject handPartPrefab;

    // Hand Parts
    [Header("Hand Parts")]
    [Tooltip("List of GameObjects representing the hand parts.")]
    [SerializeField]
    private List<GameObject> handParts = new List<GameObject>();

    [Tooltip("Scale of the hand model.")]
    [SerializeField]
    public Vector3 scale = new Vector3(1, 1, 1);

    [Tooltip("Transform representing the palm point.")]
    public Transform palmPoint;

    [Tooltip("Prefab for the hand model.")]
    public GameObject handmodel;

    // Movement Settings
    [Header("Movement Settings")]
    [Tooltip("Speed of hand movement.")]
    public float movementSpeed = 10;

    [Tooltip("GameObject representing the movement plane.")]
    public GameObject movementPlaneObject;

    [Tooltip("Enable or disable teleportation for hand movement.")]
    [SerializeField]
    private bool useTeleport = false;

    [Tooltip("Last recorded position of the hand.")]
    public Vector3 lastHandPosition;

    [Tooltip("Direction of hand movement.")]
    public Vector3 movementDirection = Vector3.zero;

    [Tooltip("Magnitude of hand movement.")]
    float movementMagnitude = 0;

    // Rotation Settings
    [Header("Rotation Settings")]
    [Tooltip("Additional rotation to apply to the hand.")]
    public Vector3 roationAddition = new Vector3(0, 0, 0);

    [Tooltip("Enable or disable wrist rotation.")]
    public bool rotateWrist = false;

    // Grabbing Settings
    [Header("Grabbing Settings")]
    [Tooltip("Threshold distance for grabbing objects.")]
    [SerializeField]
    internal float grabThreshold = 1f;

    [Tooltip("Size of the box cast for detecting grabable objects.")]
    [SerializeField]
    internal Vector3 boxCastSize = new Vector3(0.05f, 0.05f, 0.05f);

    [Tooltip("Reference to the currently grabbed object.")]
    [SerializeField]
    Grabable grabable;

    // Debug and Gizmos
    [Header("Debug and Gizmos")]
    [Tooltip("Size of the gizmo spheres.")]
    [SerializeField]
    private float gizmoSphereSize = 0.01f;

    [Tooltip("Enable or disable hand movement.")]
    public bool handCanMove = false;

    // Internal Variables
    private List<Vector3> landmarkPositions = new List<Vector3>();
    private readonly int[] fingerEndpoints = { 4, 8, 12, 16, 20 };
    private bool startedMoving = false;
    private bool isGrabbing = false;
    private int lastFingerIndex = -1;

    /// <summary>
    /// Initializes the hand model and sets its scale based on the assigned hand model.
    /// It also sets the last hand position to a default value.
    /// </summary>
    void Start()
    {
        if (handmodel != null)
        {
            scale = new Vector3(
                handmodel.transform.localScale.x * transform.localScale.x,
                handmodel.transform.localScale.y * transform.localScale.y,
                handmodel.transform.localScale.z * transform.localScale.z
            );
            Debug.Log(handmodel.transform.localScale + " " + transform.localScale + " " + scale);
        }
        else
        {
            Debug.LogWarning("Handmodel is not assigned. Using default scale.");
        }

        lastHandPosition = ConstrainToPlane(new Vector3(0.5f, 0.5f, 0.5f));
    }

    /// <summary>
    /// Updates the positions and rotations of hand parts based on the hand landmark results.
    /// It also handles the movement and rotation of the hand model.
    /// </summary>
    void Update()
    {
        try
        {
            if (handLandmarkerRunner == null)
            {
                Debug.LogError("HandLandmarkerRunner is not assigned.");
                return;
            }

            if (handLandmarkerRunner.handLandmarkerResult.handWorldLandmarks.Count > 0)
            {
                var handLandmark = handLandmarkerRunner.handLandmarkerResult.handWorldLandmarks[0];
                var landmarks = handLandmark.landmarks;
                landmarkPositions.Clear();
                for (int i = 0; i < landmarks.Count; i++)
                {
                    try
                    {
                        var landmark = landmarks[i];
                        Vector3 newpos = LandmarkToWorldPosition(landmark);
                        landmarkPositions.Add(newpos);

                        // Update the local position of the corresponding hand part
                        if (i < handParts.Count)
                        {
                            handParts[i].transform.position = newpos;

                            if (fingerEndpoints.Contains<int>(i))
                            {
                                continue;
                            }

                            // Calculate the rotation towards handParts[i+1] from handParts[i]
                            if (i + 1 < handParts.Count && handParts[i + 1] != null)
                            {
                                Vector3 direction = (LandmarkToWorldPosition(landmarks[i + 1]) - handParts[i].transform.position).normalized;
                                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                                handParts[i].transform.rotation = rotation * Quaternion.Euler(roationAddition);
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Error updating hand part {i}: {e.Message}");
                    }
                }

                if (handCanMove)
                    moveHand();
                if (rotateWrist)
                    RotateWristTowardsMidpoint();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error in Update: {e.Message}");
        }
    }

    /// <summary>
    /// Moves the hand model based on the position of the hand landmark.
    /// It updates the last hand position and calculates the movement direction and magnitude.
    /// </summary>
    private void moveHand()
    {
        if (!startedMoving)
        {
            lastHandPosition = ConstrainToPlane(multiHandLandmarkListAnnotation.PointZero);
            startedMoving = true;
            return;
        }

        // Get the new position from the landmark midpoint
        Vector3 newPosition = ConstrainToPlane(multiHandLandmarkListAnnotation.PointZero);
        Debug.ClearDeveloperConsole();
        movementDirection= newPosition - lastHandPosition;
        movementMagnitude = movementDirection.magnitude;
        movementDirection.Normalize();
        gameObject.transform.position = newPosition;
        lastHandPosition = newPosition;
    }

    /// <summary>
    /// Maps a normalized landmark position (0 to 1) to a position on the movement plane.
    /// </summary>
    /// <param name="landmark">The normalized landmark position (x, y in range 0 to 1).</param>
    /// <returns>The mapped world position on the plane.</returns>
    private Vector3 ConstrainToPlane(Landmark landmark)
    {
        return ConstrainToPlane(new Vector3(landmark.x, landmark.y, landmark.z)*10);
    }

    /// <summary>
    /// Maps a normalized position (0 to 1) to a position on the movement plane, using the X and Z axes.
    /// </summary>
    /// <param name="normalizedPosition">The normalized position (x, z in range 0 to 1).</param>
    /// <returns>The mapped world position on the plane.</returns>
    private Vector3 ConstrainToPlane(Vector3 normalizedPosition)
    {
        Renderer planeRenderer = movementPlaneObject.GetComponent<Renderer>();
        if (planeRenderer != null)
        {
            Bounds planeBounds = planeRenderer.bounds;

            // Map the normalized coordinates (0 to 1) to the plane's bounds
            float x = Mathf.Lerp(planeBounds.min.x, planeBounds.max.x, normalizedPosition.x);
            float z = Mathf.Lerp(planeBounds.min.z, planeBounds.max.z, 1.0f - normalizedPosition.y); // Flip Y-axis for Z mapping
            float y = planeBounds.center.y; // Keep Y constant (on the plane)

            return new Vector3(x, y, z);
        }

        Debug.LogError("MovementPlaneObject does not have a Renderer component.");
        return Vector3.zero;
    }

    /// <summary>
    /// Rotates the wrist to point toward the midpoint of hand parts 5 and 17.
    /// </summary>
    private void RotateWristTowardsMidpoint()
    {
        Vector3 position5 = handParts[5].transform.position;
        Vector3 position17 = handParts[17].transform.position;

        Vector3 midpoint = (position5 + position17) / 2;
        Vector3 wristPosition = handParts[0].transform.position;
        Vector3 direction = (midpoint - wristPosition).normalized;

        // Calculate the midpoint of all the fingertips
        Vector3 fingertipsMidpoint = Vector3.zero;
        int fingertipCount = 0;

        foreach (int index in fingerEndpoints)
        {
            if (index < handParts.Count && handParts[index] != null)
            {
            fingertipsMidpoint += handParts[index].transform.position;
            fingertipCount++;
            }
        }

        if (fingertipCount > 0)
        {
            fingertipsMidpoint /= fingertipCount;
        }
        Vector3 palmPosition = (position5 + position17 + wristPosition) / 3f;
        palmPoint.position = palmPosition;
        palmPoint.rotation = Quaternion.LookRotation(direction, (palmPoint.position- fingertipsMidpoint)) * Quaternion.Euler(roationAddition); 


    }

    /// <summary>
    /// Checks for collisions with grabable objects using a sphere cast.
    /// If a grabable object is detected within the grab threshold, it sets the object as grabbed.
    /// </summary>
    void FixedUpdate()
    {
        float smallestDistance = float.MaxValue;
        for (int i = 1; i < fingerEndpoints.Length; i++)
        {
            int fingertipIndex = fingerEndpoints[i];
            float distance = Vector3.Distance(handParts[fingertipIndex].transform.position, handParts[4].transform.position);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
            }
            if (distance <= grabThreshold)
            {
                if(!isGrabbing){
                    Vector3 origin = handParts[fingertipIndex].transform.position + handParts[4].transform.position;
                    origin /= 2;
                    origin.y = movementPlaneObject.transform.position.y;
                    Vector3 halfExtents = boxCastSize;
                    Quaternion orientation = Quaternion.identity;
                    Vector3 direction = (handParts[4].transform.position - origin).normalized;


                    RaycastHit hit;
                    if (Physics.SphereCast(origin, boxCastSize.x / 2, direction, out hit, boxCastSize.x, layerMask_grabable))
                    {
                        // Check if the hit object is a Grabable object
                        Debug.Log("Hit detected: " + hit.collider.name);
                        grabable = hit.collider.GetComponent<Grabable>();
                        if (grabable != null)
                        {
                            lastFingerIndex = i;
                            Debug.Log("Grabable object detected: " + grabable.name);
                            grabable.SetGrabbed(true,this);
                            isGrabbing = true;
                        }
                    }
                }
            }
        }
        if(isGrabbing && smallestDistance > grabThreshold){
            grabable.SetGrabbed(false,this);
            isGrabbing = false;
            lastFingerIndex = -1; 
        }
    }

    /// <summary>
    /// Returns the position of the hand based on the last finger index.
    /// If no finger is detected, it returns the palm point position.
    /// </summary>
    /// <returns></returns>
    public Vector3 getHandPosition(){
        if(lastFingerIndex >-1){
            Vector3 orgin =  handParts[fingerEndpoints[lastFingerIndex]].transform.position + handParts[4].transform.position;
            orgin /= 2;
            orgin.y = movementPlaneObject.transform.position.y;
            return orgin;
        }
        return palmPoint.position;
    }

    /// <summary>
    /// Converts a landmark to a world position.
    /// </summary>
    /// <param name="landmark">The landmark to convert.</param>
    /// <returns>The world position as a Vector3.</returns>
    private Vector3 LandmarkToWorldPosition(Landmark landmark)
    {
        // Use the position of the GameObject this script is attached to as the midpoint
        Vector3 midpoint = transform.position;

        // Adjust the landmark position relative to the midpoint
        return midpoint + new Vector3(-landmark.x*-1 * scale.x, -landmark.z*-1 * scale.y, -landmark.y * scale.z);
    }

    void OnDrawGizmos()
    {
        try
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < handParts.Count; i++)
            {
                if (handParts[i] != null)
                {
                    try
                    {
                        if (fingerEndpoints.Contains<int>(i))
                        {
                            continue;
                        }
                        float sphereSize = gizmoSphereSize * handParts[i].transform.localScale.magnitude;
                        Gizmos.DrawSphere(handParts[i].transform.position, sphereSize);
                        if (i < handParts.Count - 1 && handParts[i + 1] != null)
                        {
                            Gizmos.DrawLine(handParts[i].transform.position, handParts[i + 1].transform.position);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Error drawing Gizmo for hand part {i}: {e.Message}");
                    }
                }
            }

            // Draw connections between key points
            Gizmos.DrawLine(handParts[0].transform.position, handParts[5].transform.position);
            Gizmos.DrawLine(handParts[0].transform.position, handParts[17].transform.position);
            Gizmos.DrawLine(handParts[5].transform.position, handParts[9].transform.position);
            Gizmos.DrawLine(handParts[9].transform.position, handParts[13].transform.position);
            Gizmos.DrawLine(handParts[13].transform.position, handParts[17].transform.position);

            // Highlight finger endpoints with a different color
            Gizmos.color = Color.red;
            foreach (int index in fingerEndpoints)
            {
                if (index < handParts.Count && handParts[index] != null)
                {
                    try
                    {
                        float sphereSize = gizmoSphereSize * handParts[index].transform.localScale.magnitude * 1.5f; // Slightly larger for endpoints
                        Gizmos.DrawSphere(handParts[index].transform.position, sphereSize);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Error drawing Gizmo for finger endpoint {index}: {e.Message}");
                    }
                }
            }

            // Draw Gizmos for FixedUpdate logic
            if (handParts.Count > 4) 
            {
                float smallestDistance = float.MaxValue;
                Vector3 sphereCastOrigin = Vector3.zero;
                Vector3 sphereCastDirection = Vector3.zero;

                for (int i = 1; i < fingerEndpoints.Length; i++)
                {
                    int fingertipIndex = fingerEndpoints[i];
                    if (fingertipIndex < handParts.Count && handParts[fingertipIndex] != null)
                    {
                        Vector3 fingertipPosition = handParts[fingertipIndex].transform.position;
                        Vector3 thumbPosition = handParts[4].transform.position;
                        float distance = Vector3.Distance(fingertipPosition, thumbPosition);
                        if (distance < smallestDistance)
                        {
                            smallestDistance = distance;
                            sphereCastOrigin = fingertipPosition;
                            sphereCastDirection = (thumbPosition - fingertipPosition).normalized;
                        }
                        if (distance < grabThreshold)
                        {
                            Gizmos.color = Color.yellow;
                        }
                        else
                        {
                            Gizmos.color = Color.cyan;
                        }
                        Gizmos.DrawLine(fingertipPosition, thumbPosition);
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(fingertipPosition, gizmoSphereSize * 2f);
                    }
                }
                if (smallestDistance < grabThreshold)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(sphereCastOrigin, boxCastSize.x/2);
                }
            }
            if (palmPoint != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(palmPoint.position, gizmoSphereSize * 3f);
            }
            if (movementPlaneObject != null)
            {
                Renderer planeRenderer = movementPlaneObject.GetComponent<Renderer>();
                if (planeRenderer != null)
                {
                    Gizmos.color = Color.green;
                    Bounds planeBounds = planeRenderer.bounds;
                    Gizmos.DrawWireCube(planeBounds.center, planeBounds.size);
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(ConstrainToPlane(Vector3.zero), gizmoSphereSize * 5); // Bottom-left corner
                    Gizmos.DrawSphere(ConstrainToPlane(Vector3.one), gizmoSphereSize * 5); // Top-right corner
                    Gizmos.DrawSphere(ConstrainToPlane(Vector3.right), gizmoSphereSize * 5); // Bottom-right corner
                    Gizmos.DrawSphere(ConstrainToPlane(Vector3.up), gizmoSphereSize * 5); // Top-left corner
                    Gizmos.DrawSphere(ConstrainToPlane(new Vector3(0.5f, 0.5f, 0)), gizmoSphereSize * 5); // Center
                }
            }

            if (lastHandPosition != null && landmarkPositions.Count > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(lastHandPosition, lastHandPosition + movementDirection * movementMagnitude * 10);
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(lastHandPosition, gizmoSphereSize * 2f);
            }

            Gizmos.color = Color.green;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error in OnDrawGizmos: {e.Message}");
        }
    }
}
