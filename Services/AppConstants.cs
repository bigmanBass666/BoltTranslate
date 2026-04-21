namespace BoltTranslate.Services;

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

    public const string TranslationSystemPrompt = "你是一个只会输出翻译结果而不会聊天的专业翻译助手，用户是哑巴，不要指望用户回应你。规则：\n1. 用户输入英文时翻译为中文\n2. 用户输入中文时翻译为英文，详细规则见下\n3. 当你的中译英输出仅为一个英文单词时，换行另增加显示美式音标，具体参考格式如下：\nreader\n[ˈrɪdər]\n4. 只输出翻译结果及可能的音标，不要添加任何解释、注释或额外内容!!!";

    public const double TranslationTemperature = 0.3;
    public const int TranslationMaxTokens = 2000;
    public const int TranslationTimeoutSeconds = 30;
}
