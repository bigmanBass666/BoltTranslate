namespace TranslateSharp.Services;

internal static class AppConstants
{
    public const int PopupOffsetX = 20;
    public const int PopupOffsetY = 20;
    public const double PopupWidth = 400;
    public const double PopupMaxHeight = 450;

    public static readonly TimeSpan HotkeyCooldown = TimeSpan.FromMilliseconds(500);

    public const int ClipboardKeyReleaseDelay = 50;
    public const int ClipboardSimulateDelay = 30;
    public const int ClipboardPollInterval = 50;
    public const int ClipboardMaxPollCount = 10;
    public const int ClipboardJoinTimeoutMs = 8000;
    public const int ClipboardRestoreRetryCount = 5;
    public const int ClipboardRestoreRetryDelayMs = 100;

    public const string TranslationSystemPrompt = "你是一个翻译助手。将用户输入的英文文本翻译为中文。只输出翻译结果，不要添加任何解释、注释或额外内容。";

    public const double TranslationTemperature = 0.3;
    public const int TranslationMaxTokens = 2000;
    public const int TranslationTimeoutSeconds = 30;
}
