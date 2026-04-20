using System;

namespace TranslateSharp.Services;

public class HotkeyConflictException : Exception
{
    public HotkeyConflictException(string message) : base(message) { }
}
