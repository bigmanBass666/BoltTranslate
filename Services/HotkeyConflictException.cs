using System;

namespace BoltTranslate.Services;

public class HotkeyConflictException : Exception
{
    public HotkeyConflictException(string message) : base(message) { }
}
