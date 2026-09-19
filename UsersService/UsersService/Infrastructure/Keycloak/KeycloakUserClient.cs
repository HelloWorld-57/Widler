using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using UsersService.Application.Interfaces;
using UsersService.DTOs.Keycloak;

namespace UsersService.Infrastructure.Keycloak
{
    public sealed class KeycloakUserClient : IKeycloakUserClient
    {
        private readonly HttpClient _httpClient;
        private readonly KeycloakOptions _options;
        private readonly ILogger<KeycloakUserClient> _logger;

        public KeycloakUserClient(
            HttpClient httpClient,
            IOptions<KeycloakOptions> options,
            ILogger<KeycloakUserClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<KeycloakUserDto>> GetUsersAsync(CancellationToken ct)
        {
            // техдолг: добавить кэш, если запрос будет частым
            var accessToken = await GetAccessTokenAsync(ct);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var users = new List<KeycloakUserDto>();

            const int pageSize = 100;
            var first = 0;

            while (true)
            {
                var url = $"/admin/realms/{Uri.EscapeDataString(_options.Realm)}/users" +
                          $"?first={first}&max={pageSize}";

                using var response = await _httpClient.GetAsync(url, ct);

                response.EnsureSuccessStatusCode();

                var page = await response.Content.ReadFromJsonAsync<List<KeycloakUserDto>>(cancellationToken: ct) ?? [];

                users.AddRange(page);

                _logger.LogDebug("Fetched Keycloak users page. First={First}, Count={Count}", first, page.Count);

                if (page.Count < pageSize)
                    break;

                first += pageSize;
            }

            _logger.LogInformation("Fetched {Count} users from Keycloak", users.Count);

            return users;
        }

        //    private async Task<string> GetAccessTokenAsync(
        //CancellationToken ct)
        //    {
        //        var tokenUrl =
        //            $"/realms/{Uri.EscapeDataString(_options.Realm)}" +
        //            "/protocol/openid-connect/token";

        //        using var request = new HttpRequestMessage(
        //            HttpMethod.Post,
        //            tokenUrl);

        //        request.Content = new FormUrlEncodedContent(
        //        [
        //            new KeyValuePair<string, string>(
        //        "grant_type",
        //        "client_credentials"),

        //    new KeyValuePair<string, string>(
        //        "client_id",
        //        _options.ClientId),

        //    new KeyValuePair<string, string>(
        //        "client_secret",
        //        _options.ClientSecret)
        //        ]);

        //        using var response =
        //            await _httpClient.SendAsync(request, ct);

        //        var responseBody =
        //            await response.Content.ReadAsStringAsync(ct);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            _logger.LogError(
        //                "Keycloak token request failed. " +
        //                "StatusCode={StatusCode}, Response={Response}",
        //                (int)response.StatusCode,
        //                responseBody);

        //            throw new HttpRequestException(
        //                $"Keycloak token request failed with status " +
        //                $"{(int)response.StatusCode}.");
        //        }

        //        var token =
        //            JsonSerializer.Deserialize<KeycloakTokenResponse>(
        //                responseBody,
        //                new JsonSerializerOptions(JsonSerializerDefaults.Web))
        //            ?? throw new InvalidOperationException(
        //                "Keycloak token response was empty.");

        //        return token.AccessToken;
        //    }

        private async Task<string> GetAccessTokenAsync(CancellationToken ct)
        {
            var tokenUrl = $"/realms/{Uri.EscapeDataString(_options.Realm)}" +
                           "/protocol/openid-connect/token";

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);

            request.Content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _options.ClientId),
                    new KeyValuePair<string, string>("client_secret", _options.ClientSecret)
                ]);

            using var response = await _httpClient.SendAsync(request, ct);

            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken: ct)
                ?? throw new InvalidOperationException("Keycloak token response was empty.");

            return token.AccessToken;
        }
    }
}
