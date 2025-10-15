namespace Dotnet.Http.Sdk.Core
{
    /// <summary>
    /// Represents configuration options for HTTP client resiliency features such as timeouts and retries.
    /// </summary>
    public sealed class ResilienceOptions
    {
        /// <summary>
        /// Gets the timeout configuration options.
        /// </summary>
        public TimeoutOptions Timeout { get; init; } = new();

        /// <summary>
        /// Gets the retry configuration options.
        /// </summary>
        public RetryOptions Retry { get; init; } = new();
    }
}