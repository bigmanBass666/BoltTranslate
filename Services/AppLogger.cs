using System;
using System.IO;
using System.Text;

namespace BoltTranslate.Services;

public static class AppLogger
{
    private static readonly string _logDir = Path.Combine(AppContext.BaseDirectory, "logs");
    private static readonly object _lock = new();

    private static void Log(string level, string message)
    {
        if (!Directory.Exists(_logDir))
        {
            Directory.CreateDirectory(_logDir);
        }

        string fileName = $"{AppConstants.LogFilePrefix}-{DateTime.Now:yyyy-MM-dd}.log";
        string filePath = Path.Combine(_logDir, fileName);
        string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

        lock (_lock)
        {
            File.AppendAllText(filePath, logLine + Environment.NewLine, new UTF8Encoding(false));
        }
    }

    public static void Error(Exception ex, string? context = null)
    {
        string message = $"Exception: {ex.GetType().Name}, Message: {ex.Message}, StackTrace: {ex.StackTrace}";
        if (context != null)
        {
            message = context + " - " + message;
        }
        Log("ERROR", message);
    }

    public static void Warning(string message)
    {
        Log("WARN", message);
    }

    public static void Info(string message)
    {
        Log("INFO", message);
    }

    public static void Debug(string message)
    {
#if DEBUG
        Log("DEBUG", message);
#endif
    }
}
