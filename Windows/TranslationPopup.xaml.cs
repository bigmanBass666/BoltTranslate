using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using BoltTranslate.Services.NativeInterop;

namespace BoltTranslate.Windows;

public partial class TranslationPopup : Window
{
    public TranslationPopup()
    {
        InitializeComponent();
        SourceInitialized += OnSourceInitialized;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var style = Win32Api.GetWindowLong(hwnd, Win32Api.GWL_EXSTYLE);
        Win32Api.SetWindowLong(hwnd, Win32Api.GWL_EXSTYLE, style | Win32Api.WS_EX_TOOLWINDOW);
    }

    public void SetContent(string translatedText)
    {
        TranslatedTextBox.Text = translatedText;
        SizeToContent = SizeToContent.WidthAndHeight;
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Hide();
    }

    private void OnCloseClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        Hide();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.Source == CloseButton || e.LeftButton != MouseButtonState.Pressed) return;
        DragMove();
    }
}
