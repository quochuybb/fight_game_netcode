using UnityEngine;
using TMPro;

public class DebugLogToUI : MonoBehaviour
{
    public TextMeshProUGUI logText;
    private string logOutput = "";

    private void Awake()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        logOutput += $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss} [{type}] {logString}\n";
        logText.text = logOutput;
    }
}
