using UnityEngine;

public class HandControls : MonoBehaviour
{

    public float MiddleFingerThresholdPosition = 2f; //adjust this please
    [SerializeField]
    bool Grab;

    public Transform middleFinger;

    void Start()
    {
        //Kill this object if not made properly :)
        if(middleFinger == null)
        {
            Debug.LogError("Middle finger transform is not assigned. Destroying this object.");
            Destroy(this);
        }
    }

    void FixedUpdate()
    {
        if(middleFinger.position.y < MiddleFingerThresholdPosition){
            Grab = true;
        }else{
            Grab = false;
        }
    }
}
