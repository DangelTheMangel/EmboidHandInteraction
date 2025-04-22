using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using System.Linq;
using UnityEditor;
using UnityEngine.EventSystems;

public class HandPostionTransformer : MonoBehaviour
{
    [SerializeField]
    HandLandmarkerRunner handLandmarkerRunner;

    [SerializeField]
    GameObject handPartPrefab; // Prefab for hand parts (e.g., cubes)

    [SerializeField]
    private List<GameObject> handParts = new List<GameObject>(); // List to store instantiated hand parts

    [SerializeField] public Vector3 scale = new Vector3(1, 1, 1); // Scale factor for hand parts
    public Transform palmPoint;
    private List<Vector3> landmarkPositions = new List<Vector3>(); // Store landmark positions for Gizmos

    private readonly int[] fingerEndpoints = { 4, 8, 12, 16, 20 }; // Indices of finger endpoints
    public Vector3 roationAddition = new Vector3(0, 0, 0); // Rotation offset for hand parts
    public GameObject handmodel;

    public bool rotateWrist = false, handCanMove = false; // Flags for wrist rotation and hand movement

    [SerializeField]
    private float gizmoSphereSize = 0.01f; // Variable to control the size of all Gizmo spheres

    public float movementSpeed = 10;
    public GameObject movementPlaneObject;

    [SerializeField]
    private bool useTeleport = false; // Boolean to toggle teleportation

    void Start()
    {
        if (handmodel != null)
        {
            // Combine the scale of the handmodel and the parent GameObject
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

        lastHandPosition = ConstrainToPlane(new Vector3(0.5f, 0.5f, 0.5f)); // Initialize last hand position
    }
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

                // Clear previous landmark positions
                landmarkPositions.Clear();

                // Update local positions of hand parts
                for (int i = 0; i < landmarks.Count; i++)
                {
                    try
                    {
                        var landmark = landmarks[i];

                        // Convert landmark to world position
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
                                handParts[i].transform.rotation = rotation * Quaternion.Euler(roationAddition); // Apply rotation and offset
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
    public Vector3 lastHandPosition;
    bool startedMoving = false;

    public Vector3 movementDirection = Vector3.zero; // Direction of movement
    float movementMagnitude = 0; // Magnitude of movement
    private void moveHand()
    {
        if (!startedMoving)
        {
            lastHandPosition = ConstrainToPlane(handLandmarkerRunner.handLandmarkerResult.handWorldLandmarks[0].landmarks[0]);
            startedMoving = true;
            return;
        }

        // Get the new position from the landmark midpoint
        Vector3 newPosition = ConstrainToPlane(handLandmarkerRunner.handLandmarkerResult.handWorldLandmarks[0].landmarks[0]);
        Debug.ClearDeveloperConsole();
        movementDirection= newPosition - lastHandPosition;
        movementMagnitude = movementDirection.magnitude;
        movementDirection.Normalize();
        Debug.Log("New postion: " +newPosition + " Old postion: " + lastHandPosition + " Postion from mediapipe: " + handLandmarkerRunner.handLandmarkerResult.handWorldLandmarks[0].landmarks[0]);
        // Update the last hand position
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
    /// Maps a normalized position (0 to 1) to a position on the movement plane.
    /// </summary>
    /// <param name="normalizedPosition">The normalized position (x, y in range 0 to 1).</param>
    /// <returns>The mapped world position on the plane.</returns>
    private Vector3 ConstrainToPlane(Vector3 normalizedPosition)
    {
        Renderer planeRenderer = movementPlaneObject.GetComponent<Renderer>();
        if (planeRenderer != null)
        {
            Bounds planeBounds = planeRenderer.bounds;

            // Map the normalized coordinates (0 to 1) to the plane's bounds
            float x = Mathf.Lerp(planeBounds.min.x, planeBounds.max.x, normalizedPosition.x);//Mathf.Lerp(planeBounds.min.x, planeBounds.max.x, normalizedPosition.x);
            float y = Mathf.Lerp(planeBounds.min.y, planeBounds.max.y, 1.0f - normalizedPosition.y); // Flip Y-axis
            float z = planeBounds.center.z; // Keep Z constant (on the plane)

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

        // Get positions of hand parts 5 and 17
        Vector3 position5 = handParts[5].transform.position;
        Vector3 position17 = handParts[17].transform.position;

        // Calculate the midpoint
        Vector3 midpoint = (position5 + position17) / 2;
        Vector3 wristPosition = handParts[0].transform.position;
        Vector3 direction = (midpoint - wristPosition).normalized;

        // Calculate the rotation without affecting the local Y-axis rotation
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);

        // Calculate the centroid of the triangle formed by position5, position17, and wristPosition
        Vector3 palmPosition = (position5 + position17 + wristPosition) / 3f;
        palmPoint.position = palmPosition;
        palmPoint.rotation = Quaternion.Euler(lookRotation.eulerAngles.x, palmPoint.rotation.eulerAngles.y, lookRotation.eulerAngles.z);

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
        return midpoint + new Vector3(-landmark.x * scale.x, -landmark.y * scale.y, -landmark.z * scale.z);
    }

    void OnDrawGizmos()
    {
        try
        {
            // Draw Gizmos for each hand part
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

                        // Calculate sphere size based on local scale and gizmoSphereSize
                        float sphereSize = gizmoSphereSize * handParts[i].transform.localScale.magnitude;

                        // Draw a sphere at each hand part's position
                        Gizmos.DrawSphere(handParts[i].transform.position, sphereSize);

                        // Draw a line to the next hand part if it exists
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


            try
            {
                // Get positions of hand parts 5 and 17
                Vector3 position5 = handParts[5].transform.position;
                Vector3 position17 = handParts[17].transform.position;

                // Calculate the midpoint
                Vector3 midpoint = (position5 + position17) / 2;

                // Draw a sphere for hand part 5
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(position5, gizmoSphereSize * 2f); // Larger sphere for visibility

                // Draw a sphere for hand part 17
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(position17, gizmoSphereSize * 2f); // Larger sphere for visibility

                // Draw a sphere for the midpoint
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(midpoint, gizmoSphereSize * 2.5f); // Even larger sphere for the midpoint

                // Draw a line between hand part 5 and 17
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(position5, position17);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error drawing Gizmo for midpoint: {e.Message}");
            }

            // Draw the palm position
            if (palmPoint != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(palmPoint.position, gizmoSphereSize * 3f); // Larger sphere for the palm position
            }

            // Draw the bounds of the movement plane
            if (movementPlaneObject != null)
            {
                Renderer planeRenderer = movementPlaneObject.GetComponent<Renderer>();
                if (planeRenderer != null)
                {
                    Gizmos.color = Color.green;
                    Bounds planeBounds = planeRenderer.bounds;

                    // Draw the bounds as a wireframe cube
                    Gizmos.DrawWireCube(planeBounds.center, planeBounds.size);

                    Gizmos.DrawSphere(ConstrainToPlane(Vector3.zero),gizmoSphereSize*5);
                    Gizmos.DrawSphere(ConstrainToPlane(Vector3.one), gizmoSphereSize * 5);
                    Gizmos.DrawSphere(ConstrainToPlane(Vector3.right), gizmoSphereSize * 5);
                    Gizmos.DrawSphere(ConstrainToPlane(Vector3.up), gizmoSphereSize * 5);
                    Gizmos.DrawSphere(ConstrainToPlane(new Vector3(0.5f,0.5f,0)), gizmoSphereSize * 5);
                }
            }

            // Draw the new position point and movement direction
            if (lastHandPosition != null && landmarkPositions.Count > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(lastHandPosition, lastHandPosition + movementDirection * movementMagnitude*10);
                // Draw the new position as a sphere
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(lastHandPosition, gizmoSphereSize * 2f);

                // Draw the movement direction as a line
                Gizmos.color = Color.blue;
            }

            // Reset color
            Gizmos.color = Color.green;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error in OnDrawGizmos: {e.Message}");
        }
    }
}
