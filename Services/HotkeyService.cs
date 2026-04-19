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

        var (modifiers, vk) = ParseHotkey(_hotkeyString);
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

    private static (uint Modifiers, byte Vk) ParseHotkey(string hotkey)
    {
        uint modifiers = 0;
        byte vk = 0;

        var parts = hotkey.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i].ToUpperInvariant();
            if (i < parts.Length - 1)
            {
                switch (part)
                {
                    case "CTRL": case "CONTROL":
                        modifiers |= Win32Api.MOD_CONTROL; break;
                    case "SHIFT":
                        modifiers |= Win32Api.MOD_SHIFT; break;
                    case "ALT":
                        modifiers |= Win32Api.MOD_ALT; break;
                    case "WIN": case "SUPER":
                        modifiers |= Win32Api.MOD_WIN; break;
                }
            }
            else
            {
                vk = part.Length == 1 ? (byte)char.ToUpperInvariant(part[0]) : ParseVkByName(part);
            }
        }

        return (modifiers, vk > 0 ? vk : (byte)0x54);
    }

    private static byte ParseVkByName(string name)
    {
        return name.ToUpperInvariant() switch
        {
            "F1" => 0x70, "F2" => 0x71, "F3" => 0x72, "F4" => 0x73,
            "F5" => 0x74, "F6" => 0x75, "F7" => 0x76, "F8" => 0x77,
            "F9" => 0x78, "F10" => 0x79, "F11" => 0x7A, "F12" => 0x7B,
            "SPACE" => 0x20, "TAB" => 0x09, "ENTER" => 0x0D, "ESC" => 0x1B,
            "BACKSPACE" => 0x08, "DELETE" => 0x2E, "INSERT" => 0x2D,
            "HOME" => 0x24, "END" => 0x23, "PGUP" => 0x21, "PGDN" => 0x22,
            "LEFT" => 0x25, "UP" => 0x26, "RIGHT" => 0x27, "DOWN" => 0x28,
            _ => (byte)char.ToUpperInvariant(name[0])
        };
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
