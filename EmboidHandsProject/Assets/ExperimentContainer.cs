using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExperimentContainer : MonoBehaviour
{
    [Header("Experiment Settings")]
    public int participantID = 0;
    public ControlType controlType;

    [Header("UI")]
    public TMP_Dropdown controlTypeDropdown;
    public TMP_InputField participantIDInputField;

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
}