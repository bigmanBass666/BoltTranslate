namespace BoltTranslate.Services;

internal static class AppConstants
{
    public const string AppName = "BoltTranslate";
    public const string ExeName = "Bolt";
    public const string ConfigFileName = "Bolt.json";
    public const string LogFilePrefix = AppName;

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

    public const string TranslationSystemPrompt = "你是一个专业翻译助手，只输出翻译结果，不聊天，用户是哑巴不会回应。\n规则：\n1. 用户输入英文时翻译为中文\n2. 用户输入中文时翻译为英文，详细规则见下\n3. 中译英时，如果结果仅为一个英文单词，换行追加美式音标（注意：单词在前，音标在后，不要只写音标），格式如下：\nreader\n[ˈrɪdər]\n4. 翻译结果只输出内容本身，不要任何前缀、标签、解释、注释或额外内容。";

    public const double TranslationTemperature = 0.3;
    public const int TranslationMaxTokens = 2000;
    public const int TranslationTimeoutSeconds = 30;
}
