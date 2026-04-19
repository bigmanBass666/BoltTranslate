using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
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
    private IntPtr _hwnd;
    private HwndSource? _hwndSource;
    private int _hotKeyId = 0x9001;
    private bool _isRunning;
    private DateTime _lastTriggerTime = DateTime.MinValue;
    private static readonly TimeSpan Cooldown = TimeSpan.FromMilliseconds(500);

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

        GetCursorPos(out var pt);
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

    private static string GetSelectedText()
    {
        var uiaText = UiaSelectionService.TryGetSelectedText();
        if (!string.IsNullOrEmpty(uiaText))
            return uiaText;

        return GetSelectedTextViaClipboard();
    }

    private static string GetSelectedTextViaClipboard()
    {
        string savedText = "";
        string selectedText = "";

        var clipboardOp = new Thread(() =>
        {
            var seqBefore = GetClipboardSequenceNumber();

            try { savedText = System.Windows.Forms.Clipboard.GetText(); } catch { }

            var foregroundWnd = GetForegroundWindow();

            Thread.Sleep(50);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0);
            Thread.Sleep(30);

            keybd_event(VK_CONTROL, 0, 0, 0);
            keybd_event(0x43, 0, 0, 0);
            keybd_event(0x43, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(50);
                var seqAfter = GetClipboardSequenceNumber();
                if (seqAfter != seqBefore)
                    break;
            }

            try { selectedText = System.Windows.Forms.Clipboard.GetText(); } catch { }

            var seqFinal = GetClipboardSequenceNumber();
            if (seqFinal == seqBefore)
            {
                selectedText = "";
                SetForegroundWindow(foregroundWnd);
                return;
            }

            if (selectedText == savedText)
            {
                selectedText = "";
                RestoreClipboard(savedText);
                SetForegroundWindow(foregroundWnd);
                return;
            }

            RestoreClipboard(savedText);
            SetForegroundWindow(foregroundWnd);
        });
        clipboardOp.SetApartmentState(ApartmentState.STA);
        clipboardOp.Start();
        clipboardOp.Join(8000);

        return selectedText;
    }

    private static void RestoreClipboard(string text)
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                    System.Windows.Forms.Clipboard.Clear();
                else
                    System.Windows.Forms.Clipboard.SetText(text);
                return;
            }
            catch
            {
                Thread.Sleep(100);
            }
        }
    }

    #region Win32 API

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const byte VK_CONTROL = 0x11;
    private const byte VK_C = 0x43;
    private const byte VK_SHIFT = 0x10;
    private const byte VK_MENU = 0x12;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    #endregion

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
