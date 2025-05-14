using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
/// <summary>
/// This class is responsible for saving data to a CSV file.
/// It allows adding data with a header and saving the data to a specified file path.
/// </summary>
public class CSVSaver : MonoBehaviour
{
    private Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
    [SerializeField]
    private string fileName = "ParticipantData";
    [SerializeField]
    ControlType controlType;

    [SerializeField]
    private int participantNumber = 1; 

    [SerializeField]
    private string filePath;

    /// <summary>
    /// Initializes the CSVSaver by finding the ExperimentContainer and setting the participant number.
    /// It also generates the file path for saving the CSV file.
    /// </summary>
    void Start()
    {
        ExperimentContainer experimentContainer = FindAnyObjectByType<ExperimentContainer>();
        if (experimentContainer != null)
        {
            participantNumber = experimentContainer.participantID;
        }

        filePath = GetSaveFilePath(fileName, participantNumber);
    }

    /// <summary>
    /// Generates the file path for saving the CSV file.
    /// It creates a directory for CSV files if it doesn't exist.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="participantNumber"></param>
    /// <returns></returns>
    public string GetSaveFilePath(string fileName, int participantNumber)
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, "CSVFiles");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        return Path.Combine(directoryPath, $"{fileName}_Participant{participantNumber}_{controlType.ToString()}.csv");
    }

    /// <summary>
    /// Adds data to the CSV file with a specified header and value.
    /// If the header does not exist, it creates a new entry in the dictionary.
    /// </summary>
    /// <param name="header"></param>
    /// <param name="value"></param>
    public void AddData(string header, string value)
    {
        if (!data.ContainsKey(header))
        {
            data[header] = new List<string>();
        }
        data[header].Add(value);
    }

    /// <summary>
    /// Saves the CSV file to the specified file path.
    /// It writes the headers and their corresponding values to the file.
    /// </summary>
    public void SaveCSV()
    {
        SaveCSV(filePath);
    }

    /// <summary>
    /// Saves the CSV file to the specified file path.
    /// It writes the headers and their corresponding values to the file.
    /// </summary>
    /// <param name="filePath"></param>
    public void SaveCSV(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            string[] headers = new string[data.Keys.Count];
            data.Keys.CopyTo(headers, 0);
            writer.WriteLine(string.Join(";", headers));
            int rowCount = data[headers[0]].Count;
            for (int i = 0; i < rowCount; i++)
            {
                List<string> row = new List<string>();
                foreach (string header in headers)
                {
                    row.Add(data[header].Count > i ? data[header][i] : "");
                }
                writer.WriteLine(string.Join(";", row));
            }
        }
        Debug.Log($"CSV saved to: {filePath}");
    }
}

/// <summary>
/// This enum represents the type of control used in the experiment.
/// It can be either Mouse or HandTracking.
/// </summary>
[Serializable]
public enum ControlType
{
    Mouse,
    HandTracking
}
