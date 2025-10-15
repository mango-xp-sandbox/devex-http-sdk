namespace Dotnet.Http.Sdk.Core
{
    /// <summary>
    /// Provides configuration options for HTTP authentication.
    /// </summary>
    public sealed class AuthOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether authentication is enabled.
        /// When enabled, an <c>Authorization</c> header will be added to outgoing HTTP requests.
        /// </summary>
        /// <remarks>
        /// Authentication is disabled by default. Users must explicitly set this property to <c>true</c> to enable authentication.
        /// </remarks>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the delegate used to asynchronously retrieve the current access token.
        /// The returned token should not include the "Bearer " prefix.
        /// </summary>
        /// <remarks>
        /// If the delegate returns <c>null</c> or an empty string, the <c>Authorization</c> header will not be added to the
        /// request.
        /// The delegate receives a <see cref="CancellationToken" /> to support cancellation.
        /// </remarks>
        public Func<CancellationToken, Task<string>> GetTokenAsync { get; set; }
            = _ => Task.FromResult(string.Empty);
    }
}