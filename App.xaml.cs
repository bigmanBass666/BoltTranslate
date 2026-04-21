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
        AppLogger.Info($"LoadConfig completed, ApiUrl: {_config.ApiUrl}, Model: {_config.Model}, Hotkey: {_config.EffectiveHotkey}, AutoStart: {_config.AutoStart}");

        while (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            var result = System.Windows.MessageBox.Show(
                "首次使用请配置 API Key。\n\n是否现在打开 Bolt.json 进行配置？",
                "BoltTranslate",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                OpenConfigFileAndWait();
                _config = ConfigManager.Load();
            }
            else
            {
                throw new InvalidOperationException("请在 Bolt.json 中配置 ApiKey 后重启程序");
            }
        }
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

        var exePath = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "Bolt.exe");
        _autoStartService = new AutoStartService(exePath);
        if (_config.AutoStart)
            _autoStartService.Enable();
        else
            _autoStartService.Disable();
        AppLogger.Info("AutoStartService configured");

        AppLogger.Info("InitServices completed");
    }

    private async Task HandleTranslateAsync(string text, double cursorX, double cursorY)
    {
        if (_translationService == null || _windowManager == null) return;
        if (text == _lastTranslatedText) return;
        _lastTranslatedText = text;

        try
        {
            _windowManager.ShowPopupAtSelection("翻译中...", cursorX, cursorY);
            var result = await _translationService.TranslateAsync(text);
            TranslationLogger.Log(text, result);
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
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Bolt.json");
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
    }

    public AutoStartService? GetAutoStartService() => _autoStartService;
    public MainWindow? GetMainWindow() => _mainWindow;
}
