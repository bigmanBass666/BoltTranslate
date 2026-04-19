using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TranslateSharp.Services;

public interface ISelectionService
{
    void RegisterHotkey(Action<string, double, double> onTranslateRequested);
    void Start();
    void Stop();
}

public class SelectionService : ISelectionService, IDisposable
{
    private Action<string, double, double>? _onTranslateRequested;
    private readonly ITextSelectionService _textSelectionService;
    private IntPtr _hwnd;
    private HwndSource? _hwndSource;
    private int _hotKeyId = 0x9001;
    private bool _isRunning;
    private DateTime _lastTriggerTime = DateTime.MinValue;
    private static readonly TimeSpan Cooldown = TimeSpan.FromMilliseconds(500);

    public SelectionService(ITextSelectionService textSelectionService)
    {
        _textSelectionService = textSelectionService;
    }

    public void RegisterHotkey(Action<string, double, double> onTranslateRequested)
    {
        _onTranslateRequested = onTranslateRequested;
    }

    public void Start()
    {
        if (_isRunning) return;

        var helper = new WindowInteropHelper(new Window { Visibility = Visibility.Hidden });
        EnsureWindow(helper);
        _isRunning = true;
    }

    public void Stop()
    {
        if (!_isRunning) return;
        UnregisterHotkey();
        _hwndSource?.RemoveHook(WndProc);
        _hwndSource?.Dispose();
        _hwndSource = null;
        _isRunning = false;
    }

    private void EnsureWindow(WindowInteropHelper helper)
    {
        helper.EnsureHandle();
        _hwnd = helper.Handle;
        _hwndSource = HwndSource.FromHwnd(_hwnd);
        _hwndSource?.AddHook(WndProc);

        if (!RegisterHotkeyInternal())
            throw new InvalidOperationException(
                "快捷键注册失败: Ctrl+Shift+T。可能已被其他软件占用");
    }

    private bool RegisterHotkeyInternal()
    {
        return RegisterHotKey(_hwnd, _hotKeyId, MOD_CONTROL | MOD_SHIFT, 0x54);
    }

    private void UnregisterHotkey()
    {
        if (_hwnd != IntPtr.Zero)
            UnregisterHotKey(_hwnd, _hotKeyId);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        if (msg == WM_HOTKEY)
        {
            handled = true;
            OnHotkeyPressed();
        }
        return IntPtr.Zero;
    }

    private void OnHotkeyPressed()
    {
        if (DateTime.Now - _lastTriggerTime < Cooldown) return;
        _lastTriggerTime = DateTime.Now;

        GetPhysicalCursorPos(out var pt);
        double cursorX = pt.X;
        double cursorY = pt.Y;

        try
        {
            var text = GetSelectedText();
            if (!string.IsNullOrWhiteSpace(text))
                _onTranslateRequested?.Invoke(text.Trim(), cursorX, cursorY);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"获取选中文字失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private string GetSelectedText()
    {
        return _textSelectionService.GetSelectedText() ?? "";
    }

    #region Win32 API

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern bool GetPhysicalCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    #endregion

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
