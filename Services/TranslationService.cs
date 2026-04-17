using System.Net.Http;
using System.Text;
using System.Text.Json;
using TranslateSharp.Config;

namespace TranslateSharp.Services;

public interface ITranslationService
{
    Task<string> TranslateAsync(string text, CancellationToken ct = default);
    void Configure(string apiUrl, string apiKey, string model);
    bool IsConfigured { get; }
}

public class TranslationService : ITranslationService
{
    private string? _apiUrl;
    private string? _apiKey;
    private string _model = "gpt-4o-mini";
    private static readonly HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    public bool IsConfigured => !string.IsNullOrEmpty(_apiUrl) && !string.IsNullOrEmpty(_apiKey);

    public void Configure(string apiUrl, string apiKey, string model)
    {
        _apiUrl = apiUrl.TrimEnd('/');
        _apiKey = apiKey;
        _model = model;
    }

    public async Task<string> TranslateAsync(string text, CancellationToken ct = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("API 未配置，请先在 config.json 中设置 ApiUrl 和 ApiKey");

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "你是一个翻译助手。将用户输入的英文文本翻译为中文。只输出翻译结果，不要添加任何解释、注释或额外内容。" },
                new { role = "user", content = text }
            },
            temperature = 0.3,
            max_tokens = 2000
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl + "/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        try
        {
            using var response = await Client.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseBody);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?? "（翻译结果为空）";
        }
        catch (HttpRequestException ex)
        {
            throw new TranslationException($"请求失败: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new TranslationException("请求超时，请检查网络连接");
        }
        catch (KeyNotFoundException ex)
        {
            throw new TranslationException($"API 返回格式异常: {ex.Message}", ex);
        }
    }
}

public class TranslationException : Exception
{
    public TranslationException(string message, Exception? inner = null) : base(message, inner) { }
}
