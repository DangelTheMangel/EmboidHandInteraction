using System.Linq.Expressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// This class is responsible for managing the game state.
/// It handles the start and end of the game, as well as the progression through rounds.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    Bossman bossman;
    CSVSaver csvSaver;

    ExperimentContainer experimentContainer;

    [Header("UI Elements")]
    [SerializeField] GameObject startMenu;
    [SerializeField] GameObject endMenu;
    [SerializeField] GameObject gameUI;
    [SerializeField] TMP_Text objectCompletedText;

    [Header("Game Variables")]
    int currentObject= 0;
    [SerializeField] int maxObjects = 10;

    /// <summary>
    /// Initializes the game manager by finding the Bossman and CSVSaver components.
    /// It also sets the initial text for the object completed counter and activates the start menu.
    /// </summary>
    void Start()
    {
        bossman = FindAnyObjectByType<Bossman>();
        csvSaver = FindAnyObjectByType<CSVSaver>();
        objectCompletedText.text = currentObject.ToString() + "/" + maxObjects.ToString();
        startMenu.SetActive(true);
        experimentContainer = FindAnyObjectByType<ExperimentContainer>();
    }

    /// <summary>
    /// Handles the end of a round by adding data to the CSV file and updating the object completed counter.
    /// It also spawns a new shape and checks if the maximum number of objects has been reached.
    /// </summary>
    /// <param name="score"></param>
    /// <param name="time"></param>
    /// <param name="distance"></param>
    public void EndOfRound(float score, float time, float distance)
    {
        csvSaver.AddData("Score", score.ToString());
        csvSaver.AddData("Time", time.ToString());
        csvSaver.AddData("Distance", distance.ToString());

        currentObject++;
        objectCompletedText.text = currentObject.ToString() + "/" + maxObjects.ToString();
        if (currentObject >= maxObjects)
        {
            EndGame();
        }
        else
        {
            bossman.SpawnShape();
        }
    }

    /// <summary>
    /// Starts the game by spawning a shape and activating the game UI.
    /// It also deactivates the start menu and end menu.
    /// </summary>
    public void StartGame()
    {
        bossman.SpawnShape();
        startMenu.SetActive(false);
        gameUI.SetActive(true);
        endMenu.SetActive(false);
    }

    /// <summary>
    /// Ends the game by deactivating the game UI and activating the end menu.
    /// It also deactivates the start menu and saves the CSV file.
    /// </summary>
    public void EndGame()
    {
        gameUI.SetActive(false);
        endMenu.SetActive(true);
        startMenu.SetActive(false);
        csvSaver.SaveCSV();
    }

    /// <summary>
    /// Switches to the next scene in the experiment container.
    /// It checks if the experiment container is not null before proceeding.
    /// </summary>
    public void switchScene(){
        if(experimentContainer != null)
        {
            experimentContainer.nextScene();
        }
        else
        {
            Debug.LogError("ExperimentContainer not found");
        }
    }
}
