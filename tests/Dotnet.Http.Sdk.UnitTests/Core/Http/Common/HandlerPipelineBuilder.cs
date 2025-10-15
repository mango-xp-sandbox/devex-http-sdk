namespace Dotnet.Http.Sdk.UnitTests.Core.Http
{
    internal static class Pipeline
    {
        internal static HttpMessageInvoker Build(DelegatingHandler underTest, HttpMessageHandler inner)
        {
            underTest.InnerHandler = inner;
            return new HttpMessageInvoker(underTest, true);
        }

        internal static HttpRequestMessage NewRequest(HttpMethod method, string url = "https://example.test/") => new(method, url);
    }
}