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
    private const double PopupWidth = 400;
    private const double PopupMaxHeight = 450;
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
        _popup.MaxWidth = PopupWidth;
        _popup.MaxHeight = PopupMaxHeight;
        _popup.Left = 0;
        _popup.Top = 0;
        _popup.Show();

        var hwnd = new WindowInteropHelper(_popup).Handle;
        var (physicalX, physicalY) = CalculatePosition(_popupPos.X, _popupPos.Y);
        SetWindowPos(hwnd, HWND_TOPMOST, physicalX, physicalY, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
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

        var selectionBounds = UiaSelectionService.GetLastSelectionBounds();
        if (selectionBounds.HasValue)
        {
            var (left, top, right, bottom) = selectionBounds.Value;
            _popupPos = ((left + right) / 2, bottom + 5);
        }

        ShowPopup(translatedText);
    }

    public void HidePopup()
    {
        _popup.Hide();
    }

    private (int X, int Y) CalculatePosition(double physicalCx, double physicalCy)
    {
        var screenWidth = GetSystemMetrics(SM_CXSCREEN);
        var screenHeight = GetSystemMetrics(SM_CYSCREEN);

        var popupWidth = (int)_popup.ActualWidth;
        var popupHeight = (int)_popup.ActualHeight;
        if (popupWidth <= 0) popupWidth = 400;
        if (popupHeight <= 0) popupHeight = 450;

        var x = (int)(physicalCx - popupWidth / 2);
        var y = (int)(physicalCy + 5);

        if (x + popupWidth > screenWidth)
            x = screenWidth - popupWidth - 10;
        if (x < 0) x = 8;
        if (y + popupHeight > screenHeight)
            y = (int)(physicalCy - popupHeight - 10);
        if (y < 0) y = 8;

        return (x, y);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct tagPOINT { public int X; public int Y; }
}
