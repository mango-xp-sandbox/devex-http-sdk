namespace Dotnet.Http.Sdk.Core.Diagnostics
{
    public sealed class DiagnosticsOptions
    {
        /// <summary>
        /// Optional hook invoked before every request is sent.
        /// </summary>
        public Action<HttpRequestMessage>? OnRequest { get; set; }

        /// <summary>
        /// Optional hook invoked after every request completes, regardless of outcome.
        /// </summary>
        public Action<HttpRequestMessage, HttpResponseMessage>? OnResponse { get; set; }

        /// <summary>
        /// Optional hook invoked when a request throws before a response is received.
        /// </summary>
        public Action<HttpRequestMessage, Exception>? OnException { get; set; }
    }
}