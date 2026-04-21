using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BoltTranslate.Config;

namespace BoltTranslate.Windows;

public partial class SettingsWindow : Window
{
    private readonly AppConfig _config;
    private string _hotkey = "";
    private bool _isCapturingHotkey;
    private bool _apiKeyVisible;

    public SettingsWindow(AppConfig config)
    {
        _config = config;
        InitializeComponent();
        LoadConfig();
    }

    private void LoadConfig()
    {
        HotkeyTextBox.Text = _config.Hotkey;
        _hotkey = _config.Hotkey;
        ApiKeyPasswordBox.Password = _config.ApiKey;
        ModelTextBox.Text = _config.Model;
        ProxyUrlTextBox.Text = _config.ProxyUrl;
        AutoStartCheckBox.IsChecked = _config.AutoStart;
    }

    private void SaveConfig()
    {
        _config.Hotkey = _hotkey;
        _config.ApiKey = _apiKeyVisible ? ApiKeyTextBox.Text : ApiKeyPasswordBox.Password;
        _config.Model = ModelTextBox.Text;
        _config.ProxyUrl = ProxyUrlTextBox.Text;
        _config.AutoStart = AutoStartCheckBox.IsChecked == true;
        ConfigManager.Save(_config);
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }

    private void OnHotkeyGotFocus(object sender, RoutedEventArgs e)
    {
        _isCapturingHotkey = true;
        HotkeyTextBox.Text = "按下快捷键组合...";
    }

    private void OnHotkeyLostFocus(object sender, RoutedEventArgs e)
    {
        _isCapturingHotkey = false;
        if (string.IsNullOrEmpty(_hotkey))
            HotkeyTextBox.Text = _config.Hotkey;
        else
            HotkeyTextBox.Text = _hotkey;
    }

    private void OnHotkeyPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!_isCapturingHotkey) return;

        e.Handled = true;

        var modifiers = Keyboard.Modifiers;
        var key = e.Key;

        if (key == Key.System)
        {
            key = e.SystemKey;
        }

        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LeftAlt || key == Key.RightAlt ||
            key == Key.LWin || key == Key.RWin ||
            key == Key.System)
            return;

        var sb = new StringBuilder();

        if (modifiers.HasFlag(ModifierKeys.Control))
            sb.Append("Ctrl+");
        if (modifiers.HasFlag(ModifierKeys.Shift))
            sb.Append("Shift+");
        if (modifiers.HasFlag(ModifierKeys.Alt))
            sb.Append("Alt+");
        if (modifiers.HasFlag(ModifierKeys.Windows))
            sb.Append("Win+");

        sb.Append(key.ToString());

        _hotkey = sb.ToString();
        HotkeyTextBox.Text = _hotkey;
    }

    private void OnHotkeyPreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (_isCapturingHotkey && !string.IsNullOrEmpty(_hotkey))
        {
            _isCapturingHotkey = false;
            HotkeyTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }

    private void OnToggleApiVisibilityClick(object sender, RoutedEventArgs e)
    {
        _apiKeyVisible = !_apiKeyVisible;
        if (_apiKeyVisible)
        {
            ApiKeyTextBox.Text = ApiKeyPasswordBox.Password;
            ApiKeyPasswordBox.Visibility = Visibility.Collapsed;
            ApiKeyTextBox.Visibility = Visibility.Visible;
            ToggleApiKeyVisibilityButton.Content = "🙈";
        }
        else
        {
            ApiKeyPasswordBox.Password = ApiKeyTextBox.Text;
            ApiKeyTextBox.Visibility = Visibility.Collapsed;
            ApiKeyPasswordBox.Visibility = Visibility.Visible;
            ToggleApiKeyVisibilityButton.Content = "👁";
        }
    }

    private void OnApiKeyPasswordChanged(object sender, RoutedEventArgs e)
    {
    }

    private void OnApiKeyTextChanged(object sender, TextChangedEventArgs e)
    {
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        SaveConfig();
        DialogResult = true;
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }
}