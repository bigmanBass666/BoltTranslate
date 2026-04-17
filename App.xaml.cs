using System.IO;
using System.Windows;
using TranslateSharp.Config;
using TranslateSharp.Services;

namespace TranslateSharp;

public partial class App : System.Windows.Application
{
    private MainWindow? _mainWindow;
    private ITranslationService? _translationService;
    private IWindowManager? _windowManager;
    private ISelectionService? _selectionService;
    private AppConfig _config = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

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
        base.OnExit(e);
    }

    private void LoadConfig()
    {
        _config = ConfigManager.Load();
        
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            var result = System.Windows.MessageBox.Show(
                "首次使用请配置 API Key。\n\n是否现在打开 config.json 进行配置？",
                "TranslateSharp",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                OpenConfigFile();
            }

            throw new InvalidOperationException("请在 config.json 中配置 ApiKey 后重启程序");
        }
    }

    private void InitServices()
    {
        _translationService = new TranslationService();
        _translationService.Configure(_config.ApiUrl, _config.ApiKey, _config.Model);

        _windowManager = new WindowManager();

        _selectionService = new SelectionService();
        _selectionService.RegisterHotkey(async text =>
        {
            await HandleTranslateAsync(text);
        });
        _selectionService.Start();

        if (_mainWindow != null)
        {
            _mainWindow.SetServices(_translationService, _windowManager, _selectionService);
        }
    }

    private async Task HandleTranslateAsync(string text)
    {
        if (_translationService == null || _windowManager == null) return;

        try
        {
            _windowManager.ShowPopup(text, "翻译中...");
            var result = await _translationService.TranslateAsync(text);
            _windowManager.ShowPopup(text, result);
        }
        catch (TranslationException ex)
        {
            _windowManager?.ShowPopup(text, $"❌ 翻译失败: {ex.Message}");
        }
        catch (Exception ex)
        {
            _windowManager?.ShowPopup(text, $"❌ 错误: {ex.Message}");
        }
    }

    private static void OpenConfigFile()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(
            new AppConfig(), new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        using var proc = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "notepad.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
    }
}
