using BoltTranslate.Services.NativeInterop;

namespace BoltTranslate.Services;

internal static class HotkeyParser
{
    public static (uint Modifiers, byte Vk, string Normalized) Parse(string hotkey)
    {
        uint modifiers = 0;
        byte vk = 0;
        string keyPart = "";

        var parts = hotkey.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i].ToUpperInvariant();
            if (i < parts.Length - 1)
            {
                switch (part)
                {
                    case "CTRL": case "CONTROL":
                        modifiers |= Win32Api.MOD_CONTROL; break;
                    case "SHIFT":
                        modifiers |= Win32Api.MOD_SHIFT; break;
                    case "ALT":
                        modifiers |= Win32Api.MOD_ALT; break;
                    case "WIN": case "SUPER":
                        modifiers |= Win32Api.MOD_WIN; break;
                }
            }
            else
            {
                keyPart = part;
                vk = part.Length == 1 ? (byte)char.ToUpperInvariant(part[0]) : ParseVkByName(part);
            }
        }

        if (vk == 0) vk = 0x54;

        var modNames = new List<string>();
        if ((modifiers & Win32Api.MOD_CONTROL) != 0) modNames.Add("Ctrl");
        if ((modifiers & Win32Api.MOD_SHIFT) != 0) modNames.Add("Shift");
        if ((modifiers & Win32Api.MOD_ALT) != 0) modNames.Add("Alt");
        if ((modifiers & Win32Api.MOD_WIN) != 0) modNames.Add("Win");
        modNames.Add(NormalizeKeyName(keyPart));

        var normalized = string.Join("+", modNames);
        return (modifiers, vk, normalized);
    }

    public static string NormalizeHotkey(string hotkey)
    {
        var (_, _, normalized) = Parse(hotkey);
        return normalized;
    }

    private static byte ParseVkByName(string name)
    {
        return name.ToUpperInvariant() switch
        {
            "F1" => 0x70, "F2" => 0x71, "F3" => 0x72, "F4" => 0x73,
            "F5" => 0x74, "F6" => 0x75, "F7" => 0x76, "F8" => 0x77,
            "F9" => 0x78, "F10" => 0x79, "F11" => 0x7A, "F12" => 0x7B,
            "SPACE" => 0x20, "TAB" => 0x09, "ENTER" => 0x0D, "ESC" => 0x1B,
            "BACKSPACE" => 0x08, "DELETE" => 0x2E, "INSERT" => 0x2D,
            "HOME" => 0x24, "END" => 0x23, "PGUP" => 0x21, "PGDN" => 0x22,
            "LEFT" => 0x25, "UP" => 0x26, "RIGHT" => 0x27, "DOWN" => 0x28,
            _ => (byte)char.ToUpperInvariant(name[0])
        };
    }

    private static string NormalizeKeyName(string name)
    {
        var upper = name.ToUpperInvariant();
        return upper switch
        {
            "F1" => "F1", "F2" => "F2", "F3" => "F3", "F4" => "F4",
            "F5" => "F5", "F6" => "F6", "F7" => "F7", "F8" => "F8",
            "F9" => "F9", "F10" => "F10", "F11" => "F11", "F12" => "F12",
            "SPACE" => "Space", "TAB" => "Tab", "ENTER" => "Enter", "ESC" => "Esc",
            "BACKSPACE" => "Backspace", "DELETE" => "Delete", "INSERT" => "Insert",
            "HOME" => "Home", "END" => "End", "PGUP" => "PgUp", "PGDN" => "PgDn",
            "LEFT" => "Left", "UP" => "Up", "RIGHT" => "Right", "DOWN" => "Down",
            _ when upper.Length == 1 => char.ToUpperInvariant(upper[0]).ToString(),
            _ => char.ToUpperInvariant(upper[0]) + upper[1..].ToLowerInvariant()
        };
    }
}
