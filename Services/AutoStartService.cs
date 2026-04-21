using Microsoft.Win32;

namespace BoltTranslate.Services;

public class AutoStartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "BoltTranslate";
    private readonly string _exePath;

    public AutoStartService(string exePath)
    {
        _exePath = exePath;
    }

    public void Enable()
    {
        var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true)!;
        key.SetValue(ValueName, _exePath, RegistryValueKind.String);
        key.Close();
    }

    public void Disable()
    {
        var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true)!;
        key.DeleteValue(ValueName, false);
        key.Close();
    }

    public bool IsEnabled(string exePath)
    {
        var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false)!;
        var value = key.GetValue(ValueName) as string;
        key.Close();
        return value == exePath;
    }
}
