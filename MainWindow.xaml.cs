using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using TranslateSharp.Config;
using TranslateSharp.Services;

namespace TranslateSharp;

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
        _trayIcon = new NotifyIcon
        {
            Icon = CreateIcon(),
            Visible = true,
            Text = $"TranslateSharp  |  快捷键: {_config.EffectiveHotkey}"
        };

        var menu = new ContextMenuStrip();
        
        var statusItem = new ToolStripMenuItem($"快捷键: {_config.EffectiveHotkey}");
        statusItem.Enabled = false;
        menu.Items.Add(statusItem);
        menu.Items.Add(new ToolStripSeparator());
        
        var openConfigItem = new ToolStripMenuItem("打开配置文件", null, (_, _) => OpenConfigFile());
        menu.Items.Add(openConfigItem);

        var restartItem = new ToolStripMenuItem("重启", null, (_, _) => RestartApplication());
        menu.Items.Add(restartItem);

        var exitItem = new ToolStripMenuItem("退出", null, (_, _) => ExitApplication());
        menu.Items.Add(exitItem);

        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => ShowStatusBalloon();
    }

    private static Icon CreateIcon()
    {
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
            "TranslateSharp",
            $"运行中...  快捷键: {_config.EffectiveHotkey}",
            ToolTipIcon.Info);
    }

    private void OpenConfigFile()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
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
        var exePath = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "TranslateSharp.exe");
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = true
        });
        System.Windows.Application.Current.Shutdown();
    }
}
