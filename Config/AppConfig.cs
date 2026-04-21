using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BoltTranslate.Services;

namespace BoltTranslate.Config;

public class AppConfig
{
    public string ApiUrl { get; set; } = "https://open.bigmodel.cn/api/paas/v4/chat/completions";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "GLM-4-Flash-250414";
    public string ProxyUrl { get; set; } = "";

    [JsonIgnore]
    public string? HotkeyModifiers { get; set; }

    [JsonIgnore]
    public string? HotkeyKey { get; set; }

    public string Hotkey { get; set; } = "Ctrl+Shift+T";

    public bool AutoStart { get; set; }

    public string EffectiveHotkey
    {
        get
        {
            if (!string.IsNullOrEmpty(Hotkey)) return Hotkey;
            if (!string.IsNullOrEmpty(HotkeyModifiers) && !string.IsNullOrEmpty(HotkeyKey))
                return $"{HotkeyModifiers}+{HotkeyKey}";
            return "Ctrl+Shift+T";
        }
    }
}

public static class ConfigManager
{
    private static readonly string ConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, AppConstants.ConfigFileName);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static AppConfig Load()
    {
        if (!File.Exists(ConfigPath))
        {
            var defaultConfig = new AppConfig();
            Save(defaultConfig);
            return defaultConfig;
        }

        using var reader = new System.IO.StreamReader(ConfigPath, true);
        var json = reader.ReadToEnd();
        var config = JsonSerializer.Deserialize<AppConfig>(json, ReadOptions)
                   ?? new AppConfig();

        return config;
    }

    public static void Save(AppConfig config)
    {
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigPath, json, new System.Text.UTF8Encoding(false));
    }
}
