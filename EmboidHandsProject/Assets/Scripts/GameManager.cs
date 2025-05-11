using System.Linq.Expressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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
    void Start()
    {
        bossman = FindAnyObjectByType<Bossman>();
        csvSaver = FindAnyObjectByType<CSVSaver>();
        objectCompletedText.text = currentObject.ToString() + "/" + maxObjects.ToString();
        startMenu.SetActive(true);
        experimentContainer = FindAnyObjectByType<ExperimentContainer>();
    }

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

    public void StartGame()
    {
        bossman.SpawnShape();
        startMenu.SetActive(false);
        gameUI.SetActive(true);
        endMenu.SetActive(false);
    }
    public void EndGame()
    {
        gameUI.SetActive(false);
        endMenu.SetActive(true);
        startMenu.SetActive(false);
        csvSaver.SaveCSV();
    }

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
