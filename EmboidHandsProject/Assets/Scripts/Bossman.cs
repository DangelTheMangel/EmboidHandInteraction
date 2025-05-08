using System.Collections.Generic;
using UnityEngine;

public class Bossman : MonoBehaviour
{
    public List<GameObject> grabableObjects = new List<GameObject>();
    public GameObject spawnPoint;
    public GameObject endPoint;

    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.G))
        {
            SpawnShape();
        }*/
    }

    public void SpawnShape()
    {
        GameObject newObject = GameObject.Instantiate(
            grabableObjects[Random.Range(0, grabableObjects.Count)], 
            spawnPoint.transform.position, 
            spawnPoint.transform.rotation
        );

        // Set a random scale for the new object
        float randomScale = Random.Range(0.5f, 2.0f); // Adjust the range as needed
        newObject.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
    }


    public void MoveEndPoint(UnityEngine.Vector3 newPosition)
    {
        endPoint.transform.position = newPosition;
    }
}


//Start time taking
//Register when object close
//Set timer for 1sec as long as withing ray thereshold
//when timer is up give score, and store score
//Stop time taking
//Score is = Time taken, distance to endpoint