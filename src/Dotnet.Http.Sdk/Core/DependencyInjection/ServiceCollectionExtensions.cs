namespace Dotnet.Http.Sdk.Core.DependencyInjection
{
    using System.Net.Http.Headers;
    using Contacts;
    using Messages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Public;
    using SignatureVerifier;

    /// <summary>
    /// Provides extension methods for registering Sinch SDK services with the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        #region Signature Verification

        /// <summary>
        /// Adds Sinch signature verification services to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
        /// <param name="configure">
        /// An optional action to configure <see cref="SignatureVerificationOptions" />.
        /// If not provided, default options will be used.
        /// </param>
        /// <returns>
        /// The original <see cref="IServiceCollection" /> instance, for chaining.
        /// </returns>
        public static IServiceCollection AddSinchSignatureVerification(
            this IServiceCollection services,
            Action<SignatureVerificationOptions>? configure = null)
        {
            services.AddOptions<SignatureVerificationOptions>(); // If no configuration is provided, defaults will be used.
            if (configure is not null) services.Configure(configure);
            services.AddSingleton<ISinchSignatureVerifier, SinchSignatureVerifier>();
            return services;
        }

        #endregion

        #region SDK

        /// <summary>
        /// Registers the Sinch SDK and its dependencies with the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The service collection to add the Sinch SDK services to.</param>
        /// <param name="configure">An action to configure <see cref="SinchOptions" /> for the SDK.</param>
        /// <returns>The original <see cref="IServiceCollection" /> instance, for chaining.</returns>
        /// <remarks>
        /// This method configures the Sinch SDK, including HTTP client setup, authentication, and resiliency handlers.
        /// Do not set the <see cref="HttpClient.Timeout" /> property; timeouts are managed via the resiliency handler.
        /// </remarks>
        public static IServiceCollection AddSinchSdk(
            this IServiceCollection services,
            Action<SinchOptions> configure)
        {
            services.Configure(configure);

            RegisterHandlers(services);

            services.AddHttpClient(CoreConstants.HttpClientName, (sp, http) =>
                {
                    var opts = sp.GetRequiredService<IOptions<SinchOptions>>().Value;
                    http.BaseAddress = opts.BaseAddress;
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                })
                .AddHttpMessageHandler<ResilienceHandler>() // resiliency handler for retries/timeouts
                .AddHttpMessageHandler<AuthHandler>(); // bearer token handler

            RegisterCoreComponents(services);

            return services;
        }

        /// <summary>
        /// Registers the Sinch SDK and its dependencies with the specified <see cref="IServiceCollection" />,
        /// using a base URL and an asynchronous token provider for authentication.
        /// </summary>
        /// <param name="services">The service collection to add the Sinch SDK services to.</param>
        /// <param name="baseUrl">The base URL for the Sinch API endpoints.</param>
        /// <param name="tokenProvider">
        /// A delegate that asynchronously provides a bearer token for authentication.
        /// The delegate receives a <see cref="CancellationToken" /> and returns a <see cref="Task{String}" /> representing the
        /// access token.
        /// </param>
        /// <returns>The original <see cref="IServiceCollection" /> instance, for chaining.</returns>
        /// <remarks>
        /// This overload configures the Sinch SDK with a base URL and a token provider for authentication.
        /// It also enables retry and timeout resiliency by default.
        /// </remarks>
        public static IServiceCollection AddSinchSdk(
            this IServiceCollection services,
            string baseUrl,
            Func<CancellationToken, Task<string>> tokenProvider
        )
        {
            RegisterHandlers(services);

            // Register a default configuration if none is provided for the SinchOptions class.
            services.Configure<SinchOptions>(opts =>
            {
                opts.BaseAddress = new Uri(baseUrl);

                opts.Auth.Enabled = true;
                opts.Auth.UseBearerTokenAsync(tokenProvider);

                // Enable both retry and timeout resiliency by default. Resiliency and timeout configuration fallback to sane defaults.
                opts.Resilience.Retry.Enabled = true;
                opts.Resilience.Timeout.Enabled = true;
            });

            services.AddHttpClient(CoreConstants.HttpClientName, (sp, http) =>
                {
                    var opts = sp.GetRequiredService<IOptions<SinchOptions>>().Value;
                    http.BaseAddress = opts.BaseAddress;
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Setup diagnostics to log requests, responses, and exceptions to the console (showcase purposes).
                    opts.Diagnostics.OnResponse = (req, resp) =>
                        Console.WriteLine($"[{resp.StatusCode}] {req.Method} {req.RequestUri}");

                    opts.Diagnostics.OnException = (req, ex) =>
                        Console.WriteLine($"[EXCEPTION] {req.Method} {req.RequestUri}: {ex.Message}");
                })
                .AddHttpMessageHandler<ResilienceHandler>() // resiliency handler for retries/timeouts
                .AddHttpMessageHandler<AuthHandler>(); // bearer token handler

            RegisterCoreComponents(services);

            return services;
        }

        private static void RegisterHandlers(IServiceCollection services)
        {
            services.AddTransient<ResilienceHandler>();
            services.AddTransient<AuthHandler>();
        }

        private static void RegisterCoreComponents(IServiceCollection services)
        {
            services.AddTransient<IHttpGateway, HttpGateway>();
            services.AddTransient<ISinchClient, SinchClient>();
            services.AddTransient<IContactsApi, ContactsApi>();
            services.AddTransient<IMessagesApi, MessagesApi>();
        }

        #endregion
    }
}