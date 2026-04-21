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

    public const string TranslationSystemPrompt = "你是一个专业翻译助手，只输出翻译结果，不聊天，用户是哑巴不会回应。\n规则：\n1. 用户输入英文时翻译为中文\n2. 用户输入中文时翻译为英文\n3. 中译英时，如果结果仅为一个英文单词（不是专有名词、不是复合词），换行追加美式音标。格式固定为：\n单词\n[ˈrɪdər]\n注意：单词本身不能省略！格式必须是两行，第一行是单词，第二行才是音标。\n4. 专有名词（如 TranslateSharp、CornerRadius）或复合词不要加音标。\n5. 翻译结果只输出内容本身，不要任何前缀、标签、解释、注释。";

    public const double TranslationTemperature = 0.3;
    public const int TranslationMaxTokens = 2000;
    public const int TranslationTimeoutSeconds = 30;
}
