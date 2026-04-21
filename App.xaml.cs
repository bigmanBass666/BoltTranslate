using System.IO;
using System.Windows;
using System.Windows.Forms;
using BoltTranslate.Config;
using BoltTranslate.Services;
using BoltTranslate.Services.NativeInterop;

namespace BoltTranslate;

public partial class App : System.Windows.Application
{
    private MainWindow? _mainWindow;
    private ITranslationService? _translationService;
    private IWindowManager? _windowManager;
    private ISelectionService? _selectionService;
    private AutoStartService? _autoStartService;
    private AppConfig _config = null!;
    private string _lastTranslatedText = "";
    private string? _startupErrorMessage;

    protected override void OnStartup(StartupEventArgs e)
    {
        AppLogger.Info("Application starting...");

        base.OnStartup(e);
        Win32Api.FreeConsole();

        SetupExceptionHandlers();

        try
        {
            LoadConfig();
            InitServices();
            
            AppLogger.Info("Creating MainWindow...");
            _mainWindow = new MainWindow(_config, _translationService!, _windowManager!, _selectionService!);
            AppLogger.Info("MainWindow created");
            _mainWindow.Show();

            if (!string.IsNullOrEmpty(_startupErrorMessage))
            {
                _mainWindow.GetTrayIcon()?.ShowBalloonTip(
                    5000,
                    "快捷键冲突",
                    _startupErrorMessage,
                    ToolTipIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            System.Windows.Application.Current.Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppLogger.Info("Application exiting...");
        _selectionService?.Stop();
        _translationService?.Dispose();
        base.OnExit(e);
    }

    private void SetupExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
                AppLogger.Error(ex, "Unhandled domain exception");
        };

        DispatcherUnhandledException += (_, args) =>
        {
            AppLogger.Error(args.Exception, "Unhandled dispatcher exception");
            args.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            AppLogger.Error(args.Exception, "Unobserved task exception");
            args.SetObserved();
        };

        AppLogger.Info("Exception handlers registered");
    }

    private void LoadConfig()
    {
        AppLogger.Info("LoadConfig started");
        _config = ConfigManager.Load();

        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.ConfigFileName);
        var fileExists = File.Exists(configPath);
        var fileSize = fileExists ? new FileInfo(configPath).Length : 0;
        var keyPreview = string.IsNullOrWhiteSpace(_config.ApiKey) ? "(empty)" : (_config.ApiKey.Length >= 4 ? _config.ApiKey[..4] + "..." : "(too short)");
        AppLogger.Info($"Config diagnostics: path={configPath}, exists={fileExists}, size={fileSize}, apiKey={keyPreview}");

        if (string.IsNullOrWhiteSpace(_config.ApiKey))
            AppLogger.Warning("ApiKey is empty, prompting user for configuration");

        var retryCount = 0;
        while (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            retryCount++;
            AppLogger.Info($"Config retry #{retryCount}");

            var keyLineHint = "";
            try
            {
                var lines = File.ReadAllLines(configPath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Trim().StartsWith("\"ApiKey\""))
                    {
                        keyLineHint = $"\n\n第 {i + 1} 行：{lines[i].Trim()}";
                        break;
                    }
                }
            }
            catch { }

            var result = System.Windows.MessageBox.Show(
                "⚠️ 检测到 ApiKey 未配置\n\n" +
                $"文件：{configPath}\n" +
                (string.IsNullOrEmpty(keyLineHint) ? "" : keyLineHint) +
                "\n\n请在上述字段的引号内填入你的密钥，保存后关闭记事本。",
                AppConstants.AppName,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                OpenConfigFileAndWait();
                _config = ConfigManager.Load();
                keyPreview = string.IsNullOrWhiteSpace(_config.ApiKey) ? "(empty)" : (_config.ApiKey.Length >= 4 ? _config.ApiKey[..4] + "..." : "(too short)");
                AppLogger.Info($"After edit: apiKey={keyPreview}");
            }
            else
            {
                throw new InvalidOperationException("请在 " + AppConstants.ConfigFileName + " 中配置 ApiKey 后重启程序");
            }
        }

        AppLogger.Info($"LoadConfig completed, ApiUrl: {_config.ApiUrl}, Model: {_config.Model}, Hotkey: {_config.EffectiveHotkey}, AutoStart: {_config.AutoStart}");
    }

    private void InitServices()
    {
        AppLogger.Info("InitServices started");

        _translationService = new TranslationService();
        _translationService.Configure(_config.ApiUrl, _config.ApiKey, _config.Model, _config.ProxyUrl);
        AppLogger.Info("TranslationService configured");

        _windowManager = new WindowManager();
        AppLogger.Info("WindowManager created");

        var clipboardService = new ClipboardService();
        var textSelectionService = new TextSelectionService(clipboardService);

        _selectionService = new SelectionService(textSelectionService, _config);
        _selectionService.RegisterHotkey(async (text, cursorX, cursorY) =>
        {
            await HandleTranslateAsync(text, cursorX, cursorY);
        });
        AppLogger.Info("SelectionService created and hotkey registered");

        try
        {
            _selectionService.Start();
            AppLogger.Info("SelectionService.Start() succeeded");
        }
        catch (HotkeyConflictException ex)
        {
            AppLogger.Warning($"SelectionService.Start() failed: {ex.Message}");
            _startupErrorMessage = ex.Message;
        }

        var exePath = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, AppConstants.ExeName + ".exe");
        _autoStartService = new AutoStartService(exePath);
        if (_config.AutoStart)
            _autoStartService.Enable();
        else
            _autoStartService.Disable();
        AppLogger.Info("AutoStartService configured");

        AppLogger.Info("InitServices completed");
    }

    private static bool IsChineseText(string text)
    {
        return text.Any(c => c >= 0x4E00 && c <= 0x9FFF);
    }

    private static bool IsSingleChineseChar(string text)
    {
        return text.Trim().Length == 1 && text.Trim()[0] >= 0x4E00 && text.Trim()[0] <= 0x9FFF;
    }

    private static bool IsSingleWord(string text)
    {
        var word = text.Trim();
        return !word.Contains(' ') && !word.Contains('\n') && !word.Contains('\r');
    }

    private async Task<string> GetPhoneticAsync(string word)
    {
        if (_translationService == null) return "";
        try
        {
            var result = await _translationService.TranslateAsync(
                $"请给出单词 \"{word}\" 的美式音标，只返回音标，格式如：[ˈwɜːrd]，不要任何其他内容。");
            var match = System.Text.RegularExpressions.Regex.Match(result, @"\[[^\]]+\]");
            return match.Success ? match.Value : "";
        }
        catch
        {
            return "";
        }
    }

    private async Task HandleTranslateAsync(string text, double cursorX, double cursorY)
    {
        if (_translationService == null || _windowManager == null) return;
        if (text == _lastTranslatedText) return;
        _lastTranslatedText = text;

        try
        {
            _windowManager.ShowPopupAtSelection("翻译中...", cursorX, cursorY);
            var details = await _translationService.TranslateWithDetailsAsync(text);
            var result = details.TranslatedText;
            if (IsSingleChineseChar(text) && !result.Any(c => char.IsLetter(c) && (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z')))
                result = $"{text}\n{result}";
            else if (IsChineseText(text) && IsSingleWord(result) && !result.Contains('['))
            {
                var phonetic = await GetPhoneticAsync(result);
                if (!string.IsNullOrEmpty(phonetic))
                    result = $"{result}\n{phonetic}";
            }
            TranslationLogger.Log(text, result, details.Prompt, details.RawResponse);
            _windowManager.ShowPopupAtSelection(result, cursorX, cursorY);
        }
        catch (TranslationException ex)
        {
            _windowManager?.ShowPopup($"❌ 翻译失败: {ex.Message}");
        }
        catch (Exception ex)
        {
            _windowManager?.ShowPopup($"❌ 错误: {ex.Message}");
        }
    }

    private static void OpenConfigFileAndWait()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.ConfigFileName);
        if (!File.Exists(path))
        {
            File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(
                new AppConfig(), new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        }
        using var proc = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "notepad.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
        proc?.WaitForExit();

        try
        {
            var rawJson = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                AppLogger.Warning("Config file is empty after notepad edit");
            }
            else
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loaded = System.Text.Json.JsonSerializer.Deserialize<AppConfig>(rawJson, options);
                var keyOk = !string.IsNullOrWhiteSpace(loaded?.ApiKey);
                AppLogger.Info($"Notepad closed: fileSize={rawJson.Length}, apiKeyValid={keyOk}");
            }
        }
        catch (Exception ex)
        {
            AppLogger.Warning($"Failed to re-read config after notepad edit: {ex.Message}");
        }
    }

    public AutoStartService? GetAutoStartService() => _autoStartService;
    public MainWindow? GetMainWindow() => _mainWindow;
}
