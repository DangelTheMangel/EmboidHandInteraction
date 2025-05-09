using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
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

    void Start()
    {
        filePath = GetSaveFilePath(fileName, participantNumber);
    }

    public string GetSaveFilePath(string fileName, int participantNumber)
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, "CSVFiles");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        return Path.Combine(directoryPath, $"{fileName}_Participant{participantNumber}_{controlType.ToString()}.csv");
    }
    public void AddData(string header, string value)
    {
        if (!data.ContainsKey(header))
        {
            data[header] = new List<string>();
        }
        data[header].Add(value);
    }

    public void SaveCSV()
    {
        SaveCSV(filePath);
    }

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

[Serializable]
public enum ControlType
{
    Mouse,
    HandTracking
}
