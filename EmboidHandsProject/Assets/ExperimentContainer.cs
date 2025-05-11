using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    void Start()
    {
        // Populate the dropdown with all possible values of the ControlType enum
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

    private void OnControlTypeChanged(int arg0)
    {
        controlType = (ControlType)arg0;
        Debug.Log($"Control type changed to: {controlType}");
    }

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

    public void exitGameplay()
    {
        Application.Quit();
        Debug.Log("Application is exiting.");
    }


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


    public IEnumerator LoadMainScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        yield return null; 
        Destroy(gameObject); 
    }
    
}