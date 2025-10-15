namespace Dotnet.Http.Sdk.UnitTests.Core.Http
{
    using System.Net;
    using System.Net.Http.Headers;
    using Common;
    using FluentAssertions;
    using Sdk.Core;

    public class AuthHandlerTests
    {
        [Fact(DisplayName = "Auth: Adds Bearer header when token is present")]
        public async Task Adds_Bearer_header_when_token_is_present()
        {
            // Arrange
            var opts = new SinchOptions();
            opts.Auth.Enabled = true;
            opts.Auth.UseBearerToken(() => "abc123");

            var handler = new AuthHandler(new TestOptions<SinchOptions>(opts));
            var inner = new EchoOkHandler();
            using var invoker = Pipeline.Build(handler, inner);
            using var req = Pipeline.NewRequest(HttpMethod.Get);

            // Act
            var resp = await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            inner.LastRequest!.Headers.Authorization.Should().NotBeNull();
            inner.LastRequest!.Headers.Authorization!.Scheme.Should().Be("Bearer");
            inner.LastRequest!.Headers.Authorization!.Parameter.Should().Be("abc123");
        }

        [Fact(DisplayName = "Auth: Normalizes token if user prefixes with Bearer")]
        public async Task Normalizes_token_if_user_prefixes_with_Bearer()
        {
            // Arrange
            var opts = new SinchOptions();
            opts.Auth.Enabled = true;
            opts.Auth.UseBearerToken(() => "Bearer   abc123  "); // messy input

            var handler = new AuthHandler(new TestOptions<SinchOptions>(opts));
            var inner = new EchoOkHandler();
            using var invoker = Pipeline.Build(handler, inner);
            using var req = Pipeline.NewRequest(HttpMethod.Get);

            // Act
            await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            inner.LastRequest!.Headers.Authorization.Should().NotBeNull();
            inner.LastRequest!.Headers.Authorization!.Scheme.Should().Be("Bearer");
            inner.LastRequest!.Headers.Authorization!.Parameter.Should().Be("abc123");
        }

        [Fact(DisplayName = "Auth: Skips header when token is empty or null")]
        public async Task Skips_header_when_token_is_empty_or_null()
        {
            // Arrange
            var opts = new SinchOptions();
            opts.Auth.Enabled = true;
            opts.Auth.UseBearerToken(() => "   "); // empty after trim

            var handler = new AuthHandler(new TestOptions<SinchOptions>(opts));
            var inner = new EchoOkHandler();
            using var invoker = Pipeline.Build(handler, inner);
            using var req = Pipeline.NewRequest(HttpMethod.Get);

            // Act
            await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            inner.LastRequest!.Headers.Authorization.Should().BeNull();
        }

        [Fact(DisplayName = "Auth: Does not overwrite existing auth header")]
        public async Task Does_not_overwrite_existing_Authorization_header()
        {
            // Arrange
            var opts = new SinchOptions();
            opts.Auth.Enabled = true;
            opts.Auth.UseBearerToken(() => "abc123"); // would be used if header absent

            var handler = new AuthHandler(new TestOptions<SinchOptions>(opts));
            var inner = new EchoOkHandler();
            using var invoker = Pipeline.Build(handler, inner);
            using var req = Pipeline.NewRequest(HttpMethod.Get);

            // Pre-set a different scheme (escape hatch scenario)
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", "dXNlcjpzZWNyZXQ=");

            // Act
            await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            inner.LastRequest!.Headers.Authorization!.Scheme.Should().Be("Basic");
            inner.LastRequest!.Headers.Authorization!.Parameter.Should().Be("dXNlcjpzZWNyZXQ=");
        }

        [Fact(DisplayName = "Auth: Respects cancellation if token resolution is slow")]
        public async Task Respects_cancellation_before_token_resolution()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var opts = new SinchOptions();
            opts.Auth.Enabled = true;
            opts.Auth.UseBearerTokenAsync(async ct =>
            {
                // This should observe the pre-canceled token and throw immediately
                await Task.Delay(10, ct);
                return "abc123";
            });

            var handler = new AuthHandler(new TestOptions<SinchOptions>(opts));
            var inner = new EchoOkHandler();
            using var invoker = Pipeline.Build(handler, inner);
            using var req = Pipeline.NewRequest(HttpMethod.Get);

            // Act
            Func<Task> act = () => invoker.SendAsync(req, cts.Token);

            // Assert
            await act.Should().ThrowAsync<TaskCanceledException>();
            inner.LastRequest.Should().BeNull("pipeline should abort before hitting inner handler");
        }

        [Fact(DisplayName = "Auth: Exception in token delegate bubbles up")]
        public async Task Token_delegate_exception_bubbles_up()
        {
            // Arrange
            var opts = new SinchOptions();
            opts.Auth.Enabled = true;
            opts.Auth.UseBearerTokenAsync(_ => throw new InvalidOperationException("boom"));

            var handler = new AuthHandler(new TestOptions<SinchOptions>(opts));
            var inner = new EchoOkHandler();
            using var invoker = Pipeline.Build(handler, inner);
            using var req = Pipeline.NewRequest(HttpMethod.Get);

            // Act
            Func<Task> act = () => invoker.SendAsync(req, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("boom");
            inner.LastRequest.Should().BeNull();
        }
    }
}