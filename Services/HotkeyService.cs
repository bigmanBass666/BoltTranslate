using System.Windows;
using System.Windows.Interop;
using TranslateSharp.Config;
using TranslateSharp.Services.NativeInterop;

namespace TranslateSharp.Services;

public interface IHotkeyService
{
    void RegisterHotkey(Action<string> onHotkeyPressed);
    void Start();
    void Stop();
}

public class HotkeyService : IHotkeyService, IDisposable
{
    private Action<string>? _onHotkeyPressed;
    private readonly ITextSelectionService _textSelectionService;
    private readonly string _hotkeyString;
    private IntPtr _hwnd;
    private HwndSource? _hwndSource;
    private readonly int _hotKeyId = 0x9001;
    private bool _isRunning;
    private DateTime _lastTriggerTime = DateTime.MinValue;
    private static readonly TimeSpan Cooldown = AppConstants.HotkeyCooldown;

    public HotkeyService(ITextSelectionService textSelectionService, AppConfig config)
    {
        _textSelectionService = textSelectionService;
        _hotkeyString = config.EffectiveHotkey;
    }

    public void RegisterHotkey(Action<string> onHotkeyPressed)
    {
        _onHotkeyPressed = onHotkeyPressed;
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

        var (modifiers, vk, _) = HotkeyParser.Parse(_hotkeyString);
        if (!Win32Api.RegisterHotKey(_hwnd, _hotKeyId, modifiers, vk))
            throw new InvalidOperationException(
                $"快捷键注册失败: {_hotkeyString}。可能已被其他软件占用");
    }

    private void UnregisterHotkey()
    {
        if (_hwnd != IntPtr.Zero)
            Win32Api.UnregisterHotKey(_hwnd, _hotKeyId);
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
            var text = _textSelectionService.GetSelectedText();
            if (!string.IsNullOrWhiteSpace(text))
                _onHotkeyPressed?.Invoke(text.Trim());
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
