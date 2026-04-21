using System.IO;
using System.Text.Json;

namespace BoltTranslate.Services
{
    public static class TranslationLogger
    {
        public static void Log(string source, string result)
        {
            try
            {
                var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
                Directory.CreateDirectory(logsDir);

                var filePath = Path.Combine(logsDir, $"{DateTime.Now:yyyy-MM-dd}.jsonl");
                var entry = new { timestamp = DateTime.UtcNow.ToString("O"), source, result };
                var line = JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = false }) + Environment.NewLine;
                File.AppendAllText(filePath, line);
            }
            catch
            {
            }
        }
    }
}
