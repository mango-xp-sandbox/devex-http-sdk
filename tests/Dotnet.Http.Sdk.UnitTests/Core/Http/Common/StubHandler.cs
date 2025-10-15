namespace Dotnet.Http.Sdk.UnitTests.Core.Http
{
    using System.Net;

    internal sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> impl) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            var resp = await impl(request, cancellationToken);
            // Mirror real pipeline behavior
            if (resp.RequestMessage is null) resp.RequestMessage = request;
            return resp;
        }

        public static StubHandler FromResponse(HttpStatusCode status, Action<HttpResponseMessage>? mutate = null)
            => new(async (_, _) =>
            {
                var r = new HttpResponseMessage(status);
                mutate?.Invoke(r);
                return await Task.FromResult(r);
            });

        public static StubHandler FromException(Exception ex)
            => new((_, _) => Task.FromException<HttpResponseMessage>(ex));
    }
}