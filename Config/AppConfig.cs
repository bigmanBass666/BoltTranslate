using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TranslateSharp.Config;

public class AppConfig
{
    public string ApiUrl { get; set; } = "https://open.bigmodel.cn/api/paas/v4/chat/completions";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "GLM-4-Flash-250414";
    public string ProxyUrl { get; set; } = "";
    public string HotkeyModifiers { get; set; } = "Ctrl+Shift";
    public string HotkeyKey { get; set; } = "T";
}

public static class ConfigManager
{
    private static readonly string ConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static AppConfig Load()
    {
        if (!File.Exists(ConfigPath))
        {
            var config = new AppConfig();
            Save(config);
            return config;
        }

        var json = File.ReadAllText(ConfigPath);
        return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions)
               ?? new AppConfig();
    }

    public static void Save(AppConfig config)
    {
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigPath, json);
    }
}
