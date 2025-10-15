namespace Dotnet.Http.Sdk.Core
{
    /// <summary>
    /// Represents configuration options for controlling timeout behavior in HTTP operations.
    /// </summary>
    public sealed class TimeoutOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether timeout handling is enabled for HTTP operations.
        /// When set to <c>true</c>, the configured timeout will be enforced; otherwise, no timeout is applied.
        /// Disabled by default; users must explicitly opt-in.
        /// </summary>
        public bool Enabled { get; set; } = false; // Disabled by default; users must explicitly opt-in

        /// <summary>
        /// Gets or sets the overall timeout duration for an HTTP operation.
        /// This is an optimistic timeout that applies to the entire operation.
        /// Set to <c>null</c> to disable the timeout.
        /// The default value is 15 seconds.
        /// </summary>
        public TimeSpan? Overall { get; set; } = TimeSpan.FromSeconds(15);
    }
}