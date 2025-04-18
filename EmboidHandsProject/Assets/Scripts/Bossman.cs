using System.Collections.Generic;
using UnityEngine;

public class Bossman : MonoBehaviour
{
    public List<GameObject> grabableObjects = new List<GameObject>();
    public Transform spawnPoint;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            GameObject.Instantiate(grabableObjects[Random.Range(0, grabableObjects.Count)], spawnPoint.position, spawnPoint.rotation);
        }
    }
}


//Start time taking
//Register when object close
//Set timer for 1sec as long as withing ray thereshold
//when timer is up give score, and store score
//Stop time taking
//Score is = Time taken, distance to endpoint