using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class GameLogger : MonoBehaviour
{
    private static GameLogger instance;
    private string logFilePath;
    private readonly ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
    private bool isProcessingLogs = false;
    private readonly object lockObject = new object();

    [Header("Log Settings")]
    [SerializeField] private bool enableFileLogging = true;
    [SerializeField] private bool includeStackTrace = true;
    [SerializeField] private string logFilePrefix = "game_log";
    
    public static GameLogger Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameLogger");
                instance = go.AddComponent<GameLogger>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeLogger();
    }

    private void InitializeLogger()
    {
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        string userName = Environment.UserName;
        
        // Create logs directory if it doesn't exist
        string logsDirectory = Path.Combine(Application.dataPath, "Debug/Logs");
        Directory.CreateDirectory(logsDirectory);
        Debug.Log(logsDirectory);

        // Create log file with timestamp and username
        logFilePath = Path.Combine(logsDirectory, $"{logFilePrefix}_{userName}_{timestamp}.log");

        // Write initial log header
        WriteInitialLogHeader();

        // Subscribe to Unity's log messages
        Application.logMessageReceived += HandleLog;
    }

    private void WriteInitialLogHeader()
    {
        StringBuilder header = new StringBuilder();
        header.AppendLine("=================================================");
        header.AppendLine($"Log File Created: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        header.AppendLine($"User: {Environment.UserName}");
        header.AppendLine($"Unity Version: {Application.unityVersion}");
        header.AppendLine($"Platform: {Application.platform}");
        header.AppendLine($"System Language: {Application.systemLanguage}");
        header.AppendLine($"Device Model: {SystemInfo.deviceModel}");
        header.AppendLine($"Operating System: {SystemInfo.operatingSystem}");
        header.AppendLine("=================================================");
        header.AppendLine();

        File.WriteAllText(logFilePath, header.ToString());
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (!enableFileLogging) return;

        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        StringBuilder logEntry = new StringBuilder();
        
        // Format log entry
        logEntry.Append($"[{timestamp}] [{type}] ");
        logEntry.AppendLine(logString);

        if (includeStackTrace && !string.IsNullOrEmpty(stackTrace) && type != LogType.Log)
        {
            logEntry.AppendLine("Stack Trace:");
            logEntry.AppendLine(stackTrace);
            logEntry.AppendLine();
        }

        // Add to queue for async processing
        logQueue.Enqueue(logEntry.ToString());
        
        // Start processing if not already running
        if (!isProcessingLogs)
        {
            ProcessLogQueue();
        }
    }

    private async void ProcessLogQueue()
    {
        if (isProcessingLogs) return;

        lock (lockObject)
        {
            if (isProcessingLogs) return;
            isProcessingLogs = true;
        }

        await Task.Run(async () =>
        {
            while (logQueue.TryDequeue(out string logEntry))
            {
                try
                {
                    await File.AppendAllTextAsync(logFilePath, logEntry);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to write to log file: {e.Message}");
                }
            }
        });

        isProcessingLogs = false;
    }

    public void LogCustom(string message, string category = "CUSTOM")
    {
        string logMessage = $"[{category}] {message}";
        Debug.Log(logMessage);
    }

    public void LogError(string message, string category = "ERROR")
    {
        string logMessage = $"[{category}] {message}";
        Debug.LogError(logMessage);
    }

    public void LogWarning(string message, string category = "WARNING")
    {
        string logMessage = $"[{category}] {message}";
        Debug.LogWarning(logMessage);
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
}
