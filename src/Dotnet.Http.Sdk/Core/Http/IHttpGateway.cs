namespace Dotnet.Http.Sdk.Core
{
    using Public.Models;

    public interface IHttpGateway
    {
        Task<SinchResponse> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken);

        Task<SinchResponse<TResponse>> SendAsync<TResponse, TInternal>(
            HttpRequestMessage request,
            Func<Stream, CancellationToken, Task<TInternal>> deserialize,
            Func<TInternal, TResponse> map,
            CancellationToken cancellationToken);
    }
}