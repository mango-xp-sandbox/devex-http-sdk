namespace Dotnet.Http.Sdk.IntegrationTests.DependencyInjection
{
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Sdk.Core;
    using Sdk.Core.DependencyInjection;

    public class DefaultSinchSdkConfigurationTests
    {
        [Fact]
        public async Task AddSinchSdk_Without_Configuration_Should_Register_Default_Configuration()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSinchSdk("https://api.sinch.com", _ => Task.FromResult("there-is-no-key"));

            var provider = services.BuildServiceProvider();
            // Act
            var options = provider.GetRequiredService<IOptions<SinchOptions>>().Value;

            // Assert
            options.BaseAddress.Should().Be(new Uri("https://api.sinch.com/"));
            options.Auth.Enabled.Should().BeTrue();
            options.Auth.GetTokenAsync.Should().NotBeNull();

            options.Resilience.Retry.Enabled.Should().BeTrue();
            options.Resilience.Timeout.Enabled.Should().BeTrue();

            // Cleanup
            if (provider is IDisposable disposable) disposable.Dispose();
        }
    }
}