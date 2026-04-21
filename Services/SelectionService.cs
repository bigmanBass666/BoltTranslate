using System.Windows;
using System.Windows.Interop;
using BoltTranslate.Config;
using BoltTranslate.Services.NativeInterop;

namespace BoltTranslate.Services;

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
    private readonly string _hotkeyString;
    private IntPtr _hwnd;
    private HwndSource? _hwndSource;
    private readonly int _hotKeyId = 0x9001;
    private bool _isRunning;
    private DateTime _lastTriggerTime = DateTime.MinValue;
    private static readonly TimeSpan Cooldown = AppConstants.HotkeyCooldown;

    public SelectionService(ITextSelectionService textSelectionService, AppConfig config)
    {
        _textSelectionService = textSelectionService;
        _hotkeyString = config.EffectiveHotkey;
    }

    public void RegisterHotkey(Action<string, double, double> onTranslateRequested)
    {
        _onTranslateRequested = onTranslateRequested;
    }

    public void Start()
    {
        if (_isRunning)
        {
            AppLogger.Warning("SelectionService.Start() called but already running");
            return;
        }

        try
        {
            var helper = new WindowInteropHelper(new Window { Visibility = Visibility.Hidden });
            EnsureWindow(helper);
            _isRunning = true;
            AppLogger.Info($"SelectionService started, hotkey: {_hotkeyString}");
        }
        catch (Exception ex)
        {
            AppLogger.Error(ex, "SelectionService.Start() failed");
            throw;
        }
    }

    public void Stop()
    {
        if (!_isRunning) return;
        Win32Api.UnregisterHotKey(_hwnd, _hotKeyId);
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
            throw new HotkeyConflictException(
                $"快捷键 {_hotkeyString} 已被其他软件占用，请在 Bolt.json 中更换快捷键");
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

        Win32Api.GetPhysicalCursorPos(out var pt);

        try
        {
            var text = _textSelectionService.GetSelectedText();
            if (!string.IsNullOrWhiteSpace(text))
                _onTranslateRequested?.Invoke(text.Trim(), pt.X, pt.Y);
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
