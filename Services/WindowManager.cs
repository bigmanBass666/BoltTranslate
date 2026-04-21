using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using BoltTranslate.Windows;

namespace BoltTranslate.Services;

public interface IWindowManager
{
    void ShowPopup(string translatedText);
    void ShowPopup(string translatedText, double cursorX, double cursorY);
    void ShowPopupAtSelection(string translatedText, double cursorX, double cursorY);
    void HidePopup();
    bool IsVisible { get; }
}

public class WindowManager : IWindowManager
{
    private readonly TranslationPopup _popup;
    private (double X, double Y) _popupPos = (100, 100);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    public WindowManager()
    {
        _popup = new TranslationPopup();
    }

    public bool IsVisible => _popup.IsVisible;

    public void ShowPopup(string translatedText)
    {
        _popup.SetContent(translatedText);
        _popup.WindowStartupLocation = WindowStartupLocation.Manual;
        _popup.MaxWidth = AppConstants.PopupWidth;
        _popup.MaxHeight = AppConstants.PopupMaxHeight;
        _popup.Left = 0;
        _popup.Top = 0;
        _popup.Show();

        var hwnd = new WindowInteropHelper(_popup).Handle;
        var x = (int)_popupPos.X + AppConstants.PopupOffsetX;
        var y = (int)_popupPos.Y + AppConstants.PopupOffsetY;

        var screenWidth = GetSystemMetrics(SM_CXSCREEN);
        var screenHeight = GetSystemMetrics(SM_CYSCREEN);

        if (x + AppConstants.PopupWidth > screenWidth)
            x = screenWidth - (int)AppConstants.PopupWidth - 10;
        if (y + AppConstants.PopupMaxHeight > screenHeight)
            y = (int)_popupPos.Y - (int)AppConstants.PopupMaxHeight - 10;
        if (x < 0) x = 8;
        if (y < 0) y = 8;

        SetWindowPos(hwnd, HWND_TOPMOST, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
        _popup.Activate();
    }

    public void ShowPopup(string translatedText, double cursorX, double cursorY)
    {
        _popupPos = (cursorX, cursorY);
        ShowPopup(translatedText);
    }

    public void ShowPopupAtSelection(string translatedText, double cursorX, double cursorY)
    {
        _popupPos = (cursorX, cursorY);
        ShowPopup(translatedText);
    }

    public void HidePopup()
    {
        _popup.Hide();
    }
}
