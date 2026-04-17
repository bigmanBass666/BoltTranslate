using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace TranslateSharp.Windows;

public partial class TranslationPopup : Window
{
    public TranslationPopup()
    {
        InitializeComponent();
        SourceInitialized += OnSourceInitialized;
        TranslatedTextBox.PreviewMouseLeftButtonDown += OnTextBoxMouseDown;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        WindowsServices.SetWindowExStyle(hwnd, WindowsServices.WS_EX_TOOLWINDOW);
    }

    public void SetContent(string translatedText)
    {
        TranslatedTextBox.Text = translatedText;
        SizeToContent = SizeToContent.WidthAndHeight;
    }

    private void OnTextBoxMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
            DragMove();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        Hide();
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Hide();
    }
}

internal static class WindowsServices
{
    internal const int WS_EX_TOOLWINDOW = 0x00000080;
    internal const int GWL_EXSTYLE = -20;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int nIndex);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

    public static void SetWindowExStyle(IntPtr hwnd, int style)
    {
        var current = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, current | style);
    }
}
