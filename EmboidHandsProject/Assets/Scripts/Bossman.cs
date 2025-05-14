using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class is responsible for spawning and moving objects in the game.
/// It manages a list of grabable objects and allows spawning them at a specified point.
/// </summary>
public class Bossman : MonoBehaviour
{
    public List<GameObject> grabableObjects = new List<GameObject>();
    public GameObject spawnPoint;
    public GameObject endPoint;

    /// <summary>
    /// Spawns a random object from the list of grabable objects at the spawn point.
    /// </summary>
    public void SpawnShape()
    {
        GameObject newObject = GameObject.Instantiate(
            grabableObjects[Random.Range(0, grabableObjects.Count)], 
            spawnPoint.transform.position, 
            spawnPoint.transform.rotation
        );
        float randomScale = Random.Range(0.5f, 2.0f);
        newObject.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
    }

    /// <summary>
    /// Moves the spawn point to a new position.
    /// </summary>
    /// <param name="newPosition"></param>
    public void MoveEndPoint(UnityEngine.Vector3 newPosition)
    {
        endPoint.transform.position = newPosition;
    }
}