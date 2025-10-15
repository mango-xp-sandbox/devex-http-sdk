namespace Dotnet.Http.Sdk.Core
{
    /// <summary>
    /// Represents configuration options for HTTP request retry policies.
    /// </summary>
    public sealed class RetryOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether retries are enabled for idempotent HTTP methods (e.g., GET, PUT, DELETE).
        /// Disabled by default; users must explicitly opt-in.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of retry attempts to make after the initial request fails.
        /// This value does not include the initial request; it specifies the number of additional tries.
        /// </summary>
        public int Attempts { get; set; } = 3;

        /// <summary>
        /// Gets or sets the base delay <see cref="TimeSpan" /> used for calculating the backoff interval between retries.
        /// </summary>
        public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Gets or sets a value indicating whether to use decorrelated jitter for backoff delays.
        /// Jitter helps to avoid retry storms by randomizing the delay intervals.
        /// </summary>
        public bool Jitter { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether retries should be applied to HTTP POST requests.
        /// Disabled by default to avoid unintended side effects with non-idempotent operations.
        /// </summary>
        public bool ApplyToPost { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to retry requests that result in HTTP 500 Internal Server Error responses.
        /// Disabled by default.
        /// </summary>
        public bool RetryOn500 { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to honor the <c>Retry-After</c> header when present in HTTP responses.
        /// If enabled, the delay specified by the server will be respected, up to <see cref="MaxRetryAfter" />.
        /// </summary>
        public bool RespectRetryAfter { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum <see cref="TimeSpan" /> cap for honoring the <c>Retry-After</c> header.
        /// If the server's suggested delay exceeds this value, the cap will be used instead.
        /// </summary>
        public TimeSpan MaxRetryAfter { get; set; } = TimeSpan.FromSeconds(60);
    }
}