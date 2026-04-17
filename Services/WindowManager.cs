using System.Windows;
using TranslateSharp.Windows;

namespace TranslateSharp.Services;

public interface IWindowManager
{
    void ShowPopup(string originalText, string translatedText);
    void HidePopup();
    bool IsVisible { get; }
}

public class WindowManager : IWindowManager
{
    private readonly TranslationPopup _popup;
    private static readonly double MaxWidth = 420;
    private static readonly double MaxHeight = 500;

    public WindowManager()
    {
        _popup = new TranslationPopup();
    }

    public bool IsVisible => _popup.IsVisible;

    public void ShowPopup(string originalText, string translatedText)
    {
        _popup.SetContent(originalText, translatedText);
        
        var pos = CalculatePosition();
        _popup.Left = pos.X;
        _popup.Top = pos.Y;
        _popup.MaxWidth = MaxWidth;
        _popup.MaxHeight = MaxHeight;
        _popup.Show();
    }

    public void HidePopup()
    {
        _popup.Hide();
    }

    private (double X, double Y) CalculatePosition()
    {
        var cursorPos = System.Windows.Forms.Cursor.Position;
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        
        double x = cursorPos.X + 16;
        double y = cursorPos.Y + 24;

        if (x + MaxWidth > screenWidth)
            x = cursorPos.X - MaxWidth - 16;
        if (y + MaxHeight > screenHeight)
            y = cursorPos.Y - MaxHeight - 16;

        return (Math.Max(0, x), Math.Max(0, y));
    }
}
