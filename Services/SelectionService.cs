using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace TranslateSharp.Services;

public interface ISelectionService
{
    void RegisterHotkey(Action<string> onTranslateRequested);
    void Start();
    void Stop();
}

public class SelectionService : ISelectionService, IDisposable
{
    private Action<string>? _onTranslateRequested;
    private IntPtr _hwnd;
    private HwndSource? _hwndSource;
    private int _hotKeyId = 0x9001;
    private bool _isRunning;
    private DateTime _lastTriggerTime = DateTime.MinValue;
    private static readonly TimeSpan Cooldown = TimeSpan.FromMilliseconds(300);

    public void RegisterHotkey(Action<string> onTranslateRequested)
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
                "快捷键注册失败: Ctrl+Shift+T。可能已被其他软件占用，请修改 config.json 中的 HotkeyModifiers 和 HotkeyKey");
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

        try
        {
            var text = ReadClipboardText();
            if (!string.IsNullOrWhiteSpace(text))
                _onTranslateRequested?.Invoke(text.Trim());
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"读取剪贴板失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static string ReadClipboardText()
    {
        string result = "";
        var thread = new Thread(() =>
        {
            try
            {
                result = System.Windows.Forms.Clipboard.GetText();
            }
            catch { }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join(500);
        return result;
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

    #endregion

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
