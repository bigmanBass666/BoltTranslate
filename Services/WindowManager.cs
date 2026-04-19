using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using TranslateSharp.Windows;

namespace TranslateSharp.Services;

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
    private const int PopupWidth = 400;
    private const int PopupMaxHeight = 450;
    private (int X, int Y) _popupPos = (100, 100);

    public WindowManager()
    {
        _popup = new TranslationPopup();
    }

    public bool IsVisible => _popup.IsVisible;

    public void ShowPopup(string translatedText)
    {
        _popup.SetContent(translatedText);
        _popup.WindowStartupLocation = WindowStartupLocation.Manual;
        _popup.Left = -9999;
        _popup.Top = -9999;
        _popup.MaxWidth = PopupWidth;
        _popup.MaxHeight = PopupMaxHeight;
        _popup.Show();

        var (x, y) = CalculatePosition(_popupPos.X, _popupPos.Y);
        var hwnd = new WindowInteropHelper(_popup).Handle;
        if (hwnd != IntPtr.Zero)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, x, y, 0, 0, SWP_NOSIZE);
        }
        _popup.Activate();
    }

    public void ShowPopup(string translatedText, double cursorX, double cursorY)
    {
        _popupPos = ((int)cursorX, (int)cursorY);
        ShowPopup(translatedText);
    }

    public void ShowPopupAtSelection(string translatedText, double cursorX, double cursorY)
    {
        _popupPos = ((int)cursorX, (int)cursorY);

        var selectionBounds = UiaSelectionService.GetLastSelectionBounds();
        if (selectionBounds.HasValue)
        {
            var (left, top, right, bottom) = selectionBounds.Value;
            _popupPos = ((int)(left + right) / 2, (int)bottom + 5);
        }

        ShowPopup(translatedText);
    }

    public void HidePopup()
    {
        _popup.Hide();
    }

    private (int X, int Y) CalculatePosition(int cx, int cy)
    {
        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        int screenHeight = GetSystemMetrics(SM_CYSCREEN);

        int x = cx - PopupWidth / 2;
        int y = cy + 5;

        if (x + PopupWidth > screenWidth)
            x = screenWidth - PopupWidth - 10;
        if (x < 0) x = 8;
        if (y + PopupMaxHeight > screenHeight)
            y = cy - PopupMaxHeight - 10;
        if (y < 0) y = 8;

        return (x, y);
    }

    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private const uint SWP_NOSIZE = 0x0001;
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}
