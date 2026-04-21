using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace BoltTranslate.Services;

public static class TranslationLogger
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public static void Log(string source, string result, string? prompt = null, string? rawResponse = null)
    {
        try
        {
            var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logsDir);

            var filePath = Path.Combine(logsDir, $"{DateTime.Now:yyyy-MM-dd}.jsonl");
            var entry = new
            {
                timestamp = DateTime.UtcNow.ToString("O"),
                source,
                result,
                prompt = prompt ?? "",
                rawResponse = rawResponse ?? ""
            };
            var line = JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine;
            File.AppendAllText(filePath, line);
        }
        catch
        {
        }
    }
}
