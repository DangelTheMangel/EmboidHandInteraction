using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;
using UnityEngine.XR;

public class HandPostionTransformer : MonoBehaviour
{
    [SerializeField]
    HandLandmarkerRunner handLandmarkerRunner;
    [SerializeField]
    Vector3 wristPostion = new Vector3(0, 0, 0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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
                var wrist = handLandmark.landmarks[0];

                // Convert normalized screen coordinates (-1 to 1) to viewport coordinates (0 to 1)
                Vector3 viewportPosition = new Vector3(
                    (wrist.x + 1) * 0.5f, 
                    (wrist.y + 1) * 0.5f, 
                    wrist.z
                );

                // Convert viewport coordinates to world space using the main camera
                if (Camera.main != null)
                {
                    wristPostion = Camera.main.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, Camera.main.nearClipPlane + wrist.z));
                    transform.position = wristPostion;
                }
                else
                {
                    Debug.LogError("Main Camera is not assigned.");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
