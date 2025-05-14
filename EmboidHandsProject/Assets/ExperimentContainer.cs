using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// This class is responsible for managing the experiment settings.
/// It handles the participant ID, control type, and scene management.
/// </summary>
public class ExperimentContainer : MonoBehaviour
{
    [Header("Experiment Settings")]
    public int participantID = 0;
    public ControlType controlType;

    public int currentSesssion = 1;

    [Header("UI")]
    public TMP_Dropdown controlTypeDropdown;
    public TMP_InputField participantIDInputField;

    public int[] gamePlaySceneIDs = {1,2};

    /// <summary>
    /// Initializes the experiment container by setting up the control type dropdown and participant ID input field.
    /// It also ensures that the object is not destroyed when loading a new scene.
    /// </summary>
    void Start()
    {
        controlTypeDropdown.ClearOptions();
        string[] controlTypeNames = Enum.GetNames(typeof(ControlType));
        List<string> controlTypeOptions = new List<string>();
        foreach (string controlTypeName in controlTypeNames)
        {
            controlTypeOptions.Add(controlTypeName);
        }
        controlTypeDropdown.AddOptions(controlTypeOptions);

        participantIDInputField.text = participantID.ToString();
        controlTypeDropdown.onValueChanged.AddListener(OnControlTypeChanged);
        participantIDInputField.onValueChanged.AddListener(OnParticipantIDChanged);

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Handles the change in participant ID input field.
    /// It parses the input string to an integer and updates the participant ID.
    /// </summary>
    /// <param name="arg0"></param>
    private void OnParticipantIDChanged(string arg0)
    {
        if (int.TryParse(arg0, out int id))
        {
            participantID = id;
        }
        else
        {
            Debug.LogError("Invalid participant ID entered. Please enter a valid number.");
        }
    }

    /// <summary>
    /// Handles the change in control type dropdown.
    /// It updates the control type based on the selected index.
    /// </summary>
    /// <param name="arg0"></param>
    private void OnControlTypeChanged(int arg0)
    {
        controlType = (ControlType)arg0;
        Debug.Log($"Control type changed to: {controlType}");
    }

    /// <summary>
    /// Starts the gameplay by loading the appropriate scene based on the control type.
    /// It checks if the participant ID is valid before proceeding.
    /// If the participant ID is invalid, it logs an error message.
    /// </summary>
    public void StartGameplay()
    {
        if( participantID <= 0)
        {
            Debug.LogError("Invalid participant ID. Please enter a valid number.");
            return;
        }
        Debug.Log($"Stating gameplay with participant ID: {participantID} in scene {SceneManager.GetSceneByBuildIndex(gamePlaySceneIDs[(int)controlType])}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(gamePlaySceneIDs[(int)controlType]);
    }

    /// <summary>
    /// Exits the gameplay and quits the application.
    /// </summary>
    public void exitGameplay()
    {
        Application.Quit();
        Debug.Log("Application is exiting.");
    }

    /// <summary>
    /// Switches to the next scene in the experiment.
    /// It increments the current session and loads the appropriate scene based on the control type.
    /// </summary>
    public void nextScene(){
        currentSesssion++;
        if(currentSesssion > 2)
        {
            StartCoroutine(LoadMainScene());
        }else if(controlType == ControlType.HandTracking)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(gamePlaySceneIDs[0]);
        }else if(controlType == ControlType.Mouse)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(gamePlaySceneIDs[1]);
        }
    }

    /// <summary>
    /// Loads the main scene (index 0) and destroys the current game object.
    /// This method is called when the experiment is completed.
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadMainScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        yield return null; 
        Destroy(gameObject); 
    }
    
}