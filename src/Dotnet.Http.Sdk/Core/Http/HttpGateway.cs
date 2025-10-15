namespace Dotnet.Http.Sdk.Core
{
    using System.Text.Json;
    using Exceptions;
    using Internal;
    using Microsoft.Extensions.Options;
    using Polly.Timeout;
    using Public;
    using Public.Models;

    internal sealed class HttpGateway(IHttpClientFactory factory, IOptions<SinchOptions> options) : IHttpGateway
    {
        public async Task<SinchResponse> SendAsync(
            HttpRequestMessage request,
            CancellationToken ct)
        {
            var client = factory.CreateClient(CoreConstants.HttpClientName);
            try
            {
                using var resp = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                if (!resp.IsSuccessStatusCode) throw ErrorMapper.Map(resp.StatusCode, await SafeReadProblem(resp));

                var requestId = resp.Headers.TryGetValues("x-request-id", out var ids) ? ids.FirstOrDefault() ?? string.Empty : string.Empty;
                var ts = DateTimeOffset.UtcNow.ToUniversalTime().ToString("O");

                var meta = new SinchMeta(requestId, ts);

                return new SinchResponse(meta);
            }
            catch (Exception ex) when (ex is TimeoutRejectedException
                                       || ex is TaskCanceledException
                                       || ex is OperationCanceledException
                                       || ex is HttpRequestException)
            {
                throw ErrorMapper.MapTransport(ex);
            }
        }

        public async Task<SinchResponse<T>> SendAsync<T, TInternal>(
            HttpRequestMessage request,
            Func<Stream, CancellationToken, Task<TInternal>> deserialize,
            Func<TInternal, T> map,
            CancellationToken ct)
        {
            var client = factory.CreateClient(CoreConstants.HttpClientName);
            var diag = options.Value.Diagnostics;
            try
            {
                diag.OnRequest?.Invoke(request);
                using var resp = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                diag.OnResponse?.Invoke(request, resp);

                if (!resp.IsSuccessStatusCode) throw ErrorMapper.Map(resp.StatusCode, await SafeReadProblem(resp));

                var requestId = resp.Headers.TryGetValues("x-request-id", out var ids) ? ids.FirstOrDefault() ?? string.Empty : string.Empty;
                var ts = DateTimeOffset.UtcNow.ToUniversalTime().ToString("O");

                await using var stream = await resp.Content.ReadAsStreamAsync(ct);
                var data = await deserialize(stream, ct) ?? throw new InternalServerException("Deserialization returned null", 500);

                var meta = new SinchMeta(requestId, ts, BuildPagination(data));
                var response = map(data);

                return new SinchResponse<T>(response, meta);
            }
            catch (Exception ex) when (ex is TimeoutRejectedException
                                       || ex is TaskCanceledException
                                       || ex is OperationCanceledException
                                       || ex is HttpRequestException
                                       || ex is JsonException)
            {
                diag.OnException?.Invoke(request, ex);
                throw ErrorMapper.MapTransport(ex);
            }
        }

        private static SinchPagination? BuildPagination<T>(T resp)
        {
            if (resp is not PagedResponseDto dto) return null;
            return new SinchPagination(dto.Page, dto.QuantityPerPage);
        }

        private static async Task<string?> SafeReadProblem(HttpResponseMessage r)
        {
            try { return r.Content is null ? null : await r.Content.ReadAsStringAsync(); }
            catch { return null; }
        }
    }
}