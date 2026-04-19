using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using TranslateSharp.Config;
using TranslateSharp.Services;

namespace TranslateSharp;

public partial class App : System.Windows.Application
{
    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    private MainWindow? _mainWindow;
    private ITranslationService? _translationService;
    private IWindowManager? _windowManager;
    private ISelectionService? _selectionService;
    private AppConfig _config = null!;
    private string _lastTranslatedText = "";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        FreeConsole();

        try
        {
            LoadConfig();
            InitServices();
            
            _mainWindow = new MainWindow(_config);
            _mainWindow.Show();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            System.Windows.Application.Current.Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _selectionService?.Stop();
        _translationService?.Dispose();
        base.OnExit(e);
    }

    private void LoadConfig()
    {
        _config = ConfigManager.Load();

        while (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            var result = System.Windows.MessageBox.Show(
                "首次使用请配置 API Key。\n\n是否现在打开 config.json 进行配置？",
                "TranslateSharp",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                OpenConfigFileAndWait();
                _config = ConfigManager.Load();
            }
            else
            {
                throw new InvalidOperationException("请在 config.json 中配置 ApiKey 后重启程序");
            }
        }
    }

    private void InitServices()
    {
        _translationService = new TranslationService();
        _translationService.Configure(_config.ApiUrl, _config.ApiKey, _config.Model, _config.ProxyUrl);

        _windowManager = new WindowManager();

        _selectionService = new SelectionService();
        _selectionService.RegisterHotkey(async (text, cursorX, cursorY) =>
        {
            await HandleTranslateAsync(text, cursorX, cursorY);
        });
        _selectionService.Start();

        if (_mainWindow != null)
        {
            _mainWindow.SetServices(_translationService, _windowManager, _selectionService);
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
            var result = await _translationService.TranslateAsync(text);
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
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
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
}
