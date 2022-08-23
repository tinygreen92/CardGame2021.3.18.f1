/**
 * 모바일 기기에서 정상적으로 로그를 호출하려면
* [Project Setting] - [ Other Setting ] - [Scripting Define Symbols]에 ENABLE_LOGS 등록필요함
*/


#if UNITY_EDITOR || PLATFORM_ANDROID
#define ENABLE_LOGS
#endif

public sealed class Debug
{
    
    public const string ENABLE_LOGS = "ENABLE_LOGS";

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void Log(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.Log(message, context);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogFormat(string message, params object[] args)
    {
        UnityEngine.Debug.LogFormat(message, args);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogFormat(UnityEngine.Object context, string message, params object[] args)
    {
        UnityEngine.Debug.LogFormat(context, message, args);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogWarning(object message)
    {
        UnityEngine.Debug.LogWarning(message);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogWarning(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.LogWarning(message, context);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogWarningFormat(string message, params object[] args)
    {
        UnityEngine.Debug.LogWarningFormat(message, args);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogWarningFormat(UnityEngine.Object context, string message, params object[] args)
    {
        UnityEngine.Debug.LogWarningFormat(context, message, args);
    }



    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError(message);
    }



    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogError(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.LogError(message, context);
    }



    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogErrorFormat(string message, params object[] args)
    {
        UnityEngine.Debug.LogErrorFormat(message, args);
    }


    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogErrorFormat(UnityEngine.Object context, string message, params object[] args)
    {
        UnityEngine.Debug.LogErrorFormat(context, message, args);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogException(System.Exception exception)
    {
        UnityEngine.Debug.LogException(exception);
    }

    [System.Diagnostics.Conditional(ENABLE_LOGS)]
    public static void LogException(System.Exception exception, UnityEngine.Object context)
    {
        UnityEngine.Debug.LogException(exception, context);

    }
}
