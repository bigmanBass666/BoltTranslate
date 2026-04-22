using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using BoltTranslate.Config;
using BoltTranslate.Services;
using BoltTranslate.Windows;

namespace BoltTranslate;

public partial class MainWindow : Window
{
    private readonly AppConfig _config;
    private readonly ITranslationService _translationService;
    private readonly IWindowManager _windowManager;
    private readonly ISelectionService _selectionService;
    private NotifyIcon? _trayIcon;

    public MainWindow(AppConfig config, ITranslationService translationService, IWindowManager windowManager, ISelectionService selectionService)
    {
        _config = config;
        _translationService = translationService;
        _windowManager = windowManager;
        _selectionService = selectionService;
        InitializeComponent();
        AppLogger.Info($"MainWindow created, Hotkey: {_config.EffectiveHotkey}");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CreateTrayIcon();
        HideToTray();
    }

    private void OnStateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
            HideToTray();
    }

    private void CreateTrayIcon()
    {
        try
        {
            _trayIcon = new NotifyIcon
            {
                Icon = CreateIcon(),
                Visible = true,
                Text = $"{AppConstants.AppName}  |  快捷键: {_config.EffectiveHotkey}"
            };

            var menu = new ContextMenuStrip();

            var statusItem = new ToolStripMenuItem($"快捷键: {_config.EffectiveHotkey}");
            statusItem.Enabled = false;
            menu.Items.Add(statusItem);
            menu.Items.Add(new ToolStripSeparator());

            var settingsItem = new ToolStripMenuItem("设置", null, (_, _) => OpenSettings());
        menu.Items.Add(settingsItem);

        var restartItem = new ToolStripMenuItem("重启", null, (_, _) => RestartApplication());
            menu.Items.Add(restartItem);

            var exitItem = new ToolStripMenuItem("退出", null, (_, _) => ExitApplication());
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick += (_, _) => ShowStatusBalloon();
            AppLogger.Info("Tray icon created successfully");
        }
        catch (Exception ex)
        {
            AppLogger.Error(ex, "CreateTrayIcon failed");
        }
    }

    private static Icon CreateIcon()
    {
        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favicon.ico");
        if (File.Exists(iconPath))
            return new Icon(iconPath);

        using var bmp = new Bitmap(64, 64);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var brush = new SolidBrush(Color.FromArgb(0, 122, 204));
        g.FillEllipse(brush, 4, 4, 56, 56);

        using var pen = new Pen(Color.White, 3) { LineJoin = LineJoin.Round };
        g.DrawLines(pen, new[]
        {
            new PointF(20, 24), new PointF(28, 34), new PointF(44, 18)
        });

        return System.Drawing.Icon.FromHandle(bmp.GetHicon());
    }

    private void HideToTray()
    {
        Visibility = Visibility.Hidden;
        ShowInTaskbar = false;
    }

    private void ShowStatusBalloon()
    {
        _trayIcon?.ShowBalloonTip(
            2000,
            AppConstants.AppName,
            $"运行中...  快捷键: {_config.EffectiveHotkey}",
            ToolTipIcon.Info);
    }

    private void OpenConfigFile()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.ConfigFileName);
        if (!File.Exists(path))
            ConfigManager.Save(_config);

        var contentBefore = File.Exists(path) ? File.ReadAllText(path) : "";

        using var proc = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "notepad.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });

        if (proc != null)
        {
            proc.WaitForExit();
            var contentAfter = File.Exists(path) ? File.ReadAllText(path) : "";
            if (contentAfter != contentBefore)
                RestartApplication();
        }
    }

    private void ExitApplication()
    {
        _trayIcon?.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    private void RestartApplication()
    {
        _trayIcon?.Dispose();
        var exePath = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, AppConstants.ExeName + ".exe");
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = true
        });
        System.Windows.Application.Current.Shutdown();
    }

    private void OpenSettings()
    {
        try
        {
            var settingsWindow = new SettingsWindow(_config);
            var result = settingsWindow.ShowDialog();

            if (result == true)
            {
                try
                {
                    _selectionService.ReregisterHotkey(_config.EffectiveHotkey);
                    _trayIcon!.Text = $"{AppConstants.AppName}  |  快捷键: {_config.EffectiveHotkey}";
                    AppLogger.Info($"Settings saved, hotkey updated to: {_config.EffectiveHotkey}");
                }
                catch (HotkeyConflictException ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "快捷键冲突", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                ApplyAutoStartChange();
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(ex, "OpenSettings failed");
            System.Windows.MessageBox.Show("打开设置窗口失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyAutoStartChange()
    {
        var app = (App)System.Windows.Application.Current;
        var autoStartService = app.GetAutoStartService();
        if (autoStartService == null) return;

        if (_config.AutoStart)
            autoStartService.Enable();
        else
            autoStartService.Disable();
    }

    public NotifyIcon? GetTrayIcon() => _trayIcon;
}
