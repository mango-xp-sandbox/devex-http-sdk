namespace Dotnet.Http.Sdk.UnitTests.Core
{
    using Public;
    using Public.Models;
    using Sdk.Core;
    using Sdk.Core.Internal;

    internal sealed class FakeGateway : IHttpGateway
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public object? NextInternal { get; set; } // set to a wire DTO per test

        public Task<SinchResponse> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            request.RequestUri = new Uri($"http://localhost/{request.RequestUri?.OriginalString}"); // ensure absolute URI for easier assertions
            LastRequest = request;
            var meta = new SinchMeta("", DateTimeOffset.UtcNow.ToString("O"));
            return Task.FromResult(new SinchResponse(meta));
        }

        public Task<SinchResponse<T>> SendAsync<T, TInternal>(
            HttpRequestMessage request,
            Func<Stream, CancellationToken, Task<TInternal>> deserialize,
            Func<TInternal, T> map,
            CancellationToken ct)
        {
            request.RequestUri = new Uri($"http://localhost/{request.RequestUri?.OriginalString}"); // ensure absolute URI for easier assertions
            LastRequest = request;

            // We bypass real deserialization; we return the injected wire DTO.
            if (NextInternal is not TInternal dto) throw new InvalidOperationException("NextInternal not set to expected wire type.");

            var mapped = map(dto);

            var meta = new SinchMeta("", DateTimeOffset.UtcNow.ToString("O"));
            if (dto is PagedResponseDto paged) meta = new SinchMeta("", DateTimeOffset.UtcNow.ToString("O"), new SinchPagination(paged.Page, paged.QuantityPerPage));

            return Task.FromResult(new SinchResponse<T>(mapped, meta));
        }
    }
}