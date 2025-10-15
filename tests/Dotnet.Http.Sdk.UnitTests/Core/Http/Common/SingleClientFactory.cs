namespace Dotnet.Http.Sdk.UnitTests.Core.Http
{
    internal sealed class SingleClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }
}