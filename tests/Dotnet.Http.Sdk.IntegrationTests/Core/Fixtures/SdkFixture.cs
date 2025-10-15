namespace Dotnet.Http.Sdk.IntegrationTests.Core.Fixtures
{
    using Microsoft.Extensions.DependencyInjection;
    using Public;
    using Sdk.Core;
    using Sdk.Core.DependencyInjection;

    public sealed class SdkFixture : IDisposable
    {
        public SdkFixture()
        {
            var baseUrl = "http://localhost:3000";
            var token = "there-is-no-key";

            var services = new ServiceCollection();
            services.AddSinchSdk(opts =>
            {
                // Minimal configuration for tests of the SDK
                opts.BaseAddress = new Uri(baseUrl);

                opts.Auth.Enabled = true;
                opts.Auth.UseBearerToken(() => token);
            });

            Provider = services.BuildServiceProvider();
            Client = Provider.GetRequiredService<ISinchClient>();
        }

        public ServiceProvider Provider { get; }
        public ISinchClient Client { get; }
        public IMessagesApi Messages => Provider.GetRequiredService<IMessagesApi>();
        public IContactsApi Contacts => Provider.GetRequiredService<IContactsApi>();

        public void Dispose() => Provider.Dispose();
    }
}