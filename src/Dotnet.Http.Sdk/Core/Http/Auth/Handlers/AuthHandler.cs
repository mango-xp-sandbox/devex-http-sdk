namespace Dotnet.Http.Sdk.Core
{
    using System.Net.Http.Headers;
    using Microsoft.Extensions.Options;

    internal sealed class AuthHandler(IOptions<SinchOptions> options) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            // If auth is disabled, just pass through
            if (!options.Value.Auth.Enabled) return await base.SendAsync(request, ct).ConfigureAwait(false);

            // If caller already set Authorization, don't override (useful for caching scenarios, testing, etc)
            if (request.Headers.Authorization is null)
            {
                var token = (await options.Value.Auth.GetTokenAsync(ct).ConfigureAwait(false))?.Trim();
                token = NormalizeToken(token);

                if (!string.IsNullOrEmpty(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, ct).ConfigureAwait(false);
        }

        private static string NormalizeToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;

            const string prefix = "Bearer ";
            if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return token.Substring(prefix.Length).Trim();

            return token;
        }
    }
}