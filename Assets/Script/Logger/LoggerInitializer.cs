using UnityEngine;

public class LoggerInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeLogger()
    {
        GameLogger.Instance.LogCustom("Logger initialized", "SYSTEM");
    }
}