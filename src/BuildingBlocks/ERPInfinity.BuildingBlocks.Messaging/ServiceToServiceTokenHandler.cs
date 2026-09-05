using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ERPInfinity.BuildingBlocks.Messaging;

public class ServiceToServiceTokenHandler : DelegatingHandler
{
    private readonly string _identityUrl;
    private readonly string _serviceName;
    private readonly string _serviceSecret;
    private readonly List<string> _scopes;
    private static string? _cachedToken;
    private static DateTime _tokenExpiry = DateTime.MinValue;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public ServiceToServiceTokenHandler(
        string identityUrl,
        string serviceName,
        string serviceSecret,
        List<string> scopes)
    {
        _identityUrl = identityUrl;
        _serviceName = serviceName;
        _serviceSecret = serviceSecret;
        _scopes = scopes;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await GetOrFetchTokenAsync(cancellationToken);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string?> GetOrFetchTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
        {
            return _cachedToken;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            {
                return _cachedToken;
            }

            using var client = new HttpClient();
            var payload = JsonSerializer.Serialize(new
            {
                serviceName = _serviceName,
                serviceSecret = _serviceSecret,
                scopes = _scopes
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_identityUrl.TrimEnd('/')}/api/v1/auth/service-token", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(json);
                _cachedToken = doc.RootElement.GetProperty("token").GetString();
                _tokenExpiry = doc.RootElement.GetProperty("expiresAt").GetDateTime();
                return _cachedToken;
            }
        }
        catch
        {
            // Fallback for offline/test environments
        }
        finally
        {
            _semaphore.Release();
        }

        return _cachedToken;
    }
}
