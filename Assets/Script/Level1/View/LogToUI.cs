using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LogToUI : MonoBehaviour
{
    public Text logText; // Прив’яжи сюди Text або TextMeshProUGUI
    private List<string> logLines = new List<string>();
    private const int maxLines = 100;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        logLines.Add(logString);
        if (logLines.Count > maxLines)
            logLines.RemoveAt(0);

        logText.text = string.Join("\n", logLines);
    }
}
