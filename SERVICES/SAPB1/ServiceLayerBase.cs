using DTO;
using DTO.SAPB1;
using DTO.SAPB1ServiceLayer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SERVICES.SAPB1
{
    public abstract class ServiceLayerBase
    {
        protected readonly ServiceLayerSettings _settings;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly IMemoryCache _cache;
        private readonly string _cacheKey;

        protected ServiceLayerBase(IOptions<ServiceLayerSettings> options, IHttpClientFactory httpClientFactory,
            IMemoryCache cache)
        {
            _settings = options.Value;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _cacheKey = $"SL_Session_{_settings.CompanyDB}";
        }

        private async Task<string> GetSessionAsync()
        {
            if (!_cache.TryGetValue(_cacheKey, out string? sessionId))
            {
                var loginResult = await LoginAsync();
                if (!loginResult.Exito) throw new UnauthorizedAccessException(loginResult.Error?.Msg);

                sessionId = loginResult.Datos?.SessionId ?? string.Empty;

                var timeout = (loginResult.Datos?.SessionTimeout ?? 30) - 5;
                _cache.Set(_cacheKey, sessionId, TimeSpan.FromMinutes(timeout));
            }
            return sessionId!;
        }

        private async Task<ResultOp<ServiceLayerSession>> LoginAsync()
        {
            var client = _httpClientFactory.CreateClient("ServiceLayerClient");
            var loginData = new { _settings.CompanyDB, _settings.UserName, _settings.Password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Login", content);
            if (!response.IsSuccessStatusCode) return ResultOp<ServiceLayerSession>.Fallo("Login SL Fallido");

            var json = await response.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<ServiceLayerSession>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return ResultOp<ServiceLayerSession>.Ok(session!);
        }

        private string ExtraerErrorSAP(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                if (doc.RootElement.TryGetProperty("error", out var errorElement) &&
                    errorElement.TryGetProperty("message", out var messageElement) &&
                    messageElement.TryGetProperty("value", out var valueElement))
                {
                    return valueElement.GetString() ?? jsonResponse;
                }
            }
            catch
            {
            }
            return jsonResponse;
        }

        protected async Task<ResultOp<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var sessionId = await GetSessionAsync();
                var client = _httpClientFactory.CreateClient("ServiceLayerClient");

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("Cookie", $"B1SESSION={sessionId}");
                request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return ResultOp<TResponse>.Ok(result!);
                }

                string errorSAP = ExtraerErrorSAP(jsonResponse);
                return ResultOp<TResponse>.Fallo($"Error en SAP: {errorSAP}");
            }
            catch (Exception ex)
            {
                return ResultOp<TResponse>.Fallo($"Excepción en ServiceLayer: {ex.Message}");
            }
        }

        protected async Task<ResultOp<TResponse>> GetAsync<TResponse>(string endpoint)
        {
            try
            {
                var sessionId = await GetSessionAsync();
                var client = _httpClientFactory.CreateClient("ServiceLayerClient");

                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Add("Cookie", $"B1SESSION={sessionId}");

                var response = await client.SendAsync(request);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return ResultOp<TResponse>.Ok(result!);
                }

                string errorSAP = ExtraerErrorSAP(jsonResponse);
                return ResultOp<TResponse>.Fallo($"Error GET en SAP: {errorSAP}");
            }
            catch (Exception ex)
            {
                return ResultOp<TResponse>.Fallo($"Excepción en ServiceLayer: {ex.Message}");
            }
        }

}
}