namespace Dotnet.Http.Sdk.Core
{
    using Diagnostics;

    /// <summary>
    /// Represents the root configuration options for the Sinch HTTP SDK.
    /// </summary>
    public sealed class SinchOptions
    {
        /// <summary>
        /// Gets or sets the base address for HTTP requests.
        /// Defaults to <c>http://localhost:3000/</c>.
        /// </summary>
        public Uri BaseAddress { get; set; } = new("http://localhost:3000/");

        /// <summary>
        /// Gets the authentication configuration options.
        /// </summary>
        public AuthOptions Auth { get; init; } = new();

        /// <summary>
        /// Gets the resiliency configuration options, such as timeouts and retries.
        /// </summary>
        public ResilienceOptions Resilience { get; init; } = new();

        /// <summary>
        /// Gets the diagnostics configuration options.
        /// </summary>
        public DiagnosticsOptions Diagnostics { get; } = new();
    }
}