using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using TranslateSharp.Config;

namespace TranslateSharp.Services;

public interface ITranslationService : IDisposable
{
    Task<string> TranslateAsync(string text, CancellationToken ct = default);
    void Configure(string apiUrl, string apiKey, string model, string proxyUrl = "");
    bool IsConfigured { get; }
}

public class TranslationService : ITranslationService
{
    private string? _apiUrl;
    private string? _apiKey;
    private string _model = "gpt-4o-mini";
    private HttpClient? _client;
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(AppConstants.TranslationTimeoutSeconds);

    public bool IsConfigured => !string.IsNullOrEmpty(_apiUrl) && !string.IsNullOrEmpty(_apiKey);

    public void Configure(string apiUrl, string apiKey, string model, string proxyUrl = "")
    {
        _client?.Dispose();

        var handler = new SocketsHttpHandler
        {
            ConnectTimeout = Timeout,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        };

        if (!string.IsNullOrWhiteSpace(proxyUrl))
        {
            try
            {
                handler.Proxy = new WebProxy(proxyUrl);
                handler.UseProxy = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"代理地址无效: {proxyUrl} - {ex.Message}");
            }
        }

        _client = new HttpClient(handler)
        {
            Timeout = Timeout
        };

        _apiUrl = apiUrl.Trim();
        _apiKey = apiKey;
        _model = model;
    }

    public void Dispose()
    {
        _client?.Dispose();
        _client = null;
    }

    public async Task<string> TranslateAsync(string text, CancellationToken ct = default)
    {
        if (!IsConfigured || _client == null)
            throw new InvalidOperationException("API 未配置，请先在 config.json 中设置 ApiUrl 和 ApiKey");

        if (string.IsNullOrWhiteSpace(text))
            return "";

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = AppConstants.TranslationSystemPrompt },
                new { role = "user", content = text }
            },
            temperature = AppConstants.TranslationTemperature,
            max_tokens = AppConstants.TranslationMaxTokens
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        try
        {
            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = ParseErrorResponse(responseBody, response.StatusCode);
                throw new TranslationException($"API 错误 ({(int)response.StatusCode}): {errorMsg}");
            }

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
            throw new TranslationException($"网络请求失败: {ex.Message}\n\n可能原因:\n- 网络未连接\n- API 地址错误\n- 需要配置代理 (ProxyUrl)", ex);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TranslationException("请求超时 (30秒)。请检查网络连接或配置代理");
        }
        catch (OperationCanceledException)
        {
            throw new TranslationException("请求已取消");
        }
        catch (JsonException ex)
        {
            throw new TranslationException($"API 返回数据解析失败，可能是返回格式不兼容: {ex.Message}", ex);
        }
    }

    private static string ParseErrorResponse(string body, HttpStatusCode statusCode)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var message = error.GetProperty("message").GetString();
                var code = error.TryGetProperty("code", out var c) ? c.GetString() : "";
                return !string.IsNullOrWhiteSpace(code) ? $"[{code}] {message}" : message ?? body;
            }
        }
        catch { }

        if (body.Length > 300)
            return body[..300] + "...";

        return body;
    }
}

public class TranslationException : Exception
{
    public TranslationException(string message, Exception? inner = null) : base(message, inner) { }
}
