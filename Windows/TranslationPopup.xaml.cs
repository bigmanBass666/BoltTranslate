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

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Hide();
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (!string.IsNullOrEmpty(TranslatedTextBox.Text))
        {
            System.Windows.Clipboard.SetText(TranslatedTextBox.Text);
            CopyButton.Content = "✓";
            CopyButton.Foreground = System.Windows.Media.Brushes.Green;
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            timer.Tick += (s, _) =>
            {
                CopyButton.Content = "📋";
                CopyButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x33, 0x33, 0x33));
                timer.Stop();
            };
            timer.Start();
        }
    }

    private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.Source == CloseButton || e.Source == CopyButton) return;
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (e.ClickCount == 2)
                Hide();
            else
                DragMove();
        }
    }
}
