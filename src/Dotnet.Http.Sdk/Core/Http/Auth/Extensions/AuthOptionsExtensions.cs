namespace Dotnet.Http.Sdk.Core
{
    /// <summary>
    /// Provides extension methods for configuring <see cref="AuthOptions" /> with bearer token authentication.
    /// </summary>
    public static class AuthOptionsExtensions
    {
        /// <summary>
        /// Configures the <see cref="AuthOptions" /> to use a synchronous bearer token provider.
        /// </summary>
        /// <param name="opts">The <see cref="AuthOptions" /> instance to configure.</param>
        /// <param name="tokenFactory">
        /// A delegate that returns the current access token as a <see cref="string" />.
        /// The returned token should not include the "Bearer " prefix.
        /// </param>
        /// <remarks>
        /// The provided <paramref name="tokenFactory" /> will be wrapped in a <see cref="Task" /> to support asynchronous usage.
        /// </remarks>
        public static void UseBearerToken(this AuthOptions opts, Func<string> tokenFactory)
            => opts.GetTokenAsync = _ => Task.FromResult(tokenFactory());

        /// <summary>
        /// Configures the <see cref="AuthOptions" /> to use an asynchronous bearer token provider.
        /// </summary>
        /// <param name="opts">The <see cref="AuthOptions" /> instance to configure.</param>
        /// <param name="tokenFactory">
        /// A delegate that asynchronously returns the current access token as a <see cref="string" />.
        /// The returned token should not include the "Bearer " prefix.
        /// The delegate receives a <see cref="CancellationToken" /> to support cancellation.
        /// </param>
        public static void UseBearerTokenAsync(this AuthOptions opts, Func<CancellationToken, Task<string>> tokenFactory)
            => opts.GetTokenAsync = tokenFactory;
    }
}