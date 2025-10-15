namespace Dotnet.Http.Sdk.UnitTests.Core.Http
{
    using System.Net;
    using System.Net.Http.Headers;
    using FluentAssertions;
    using Polly.Timeout;
    using Sdk.Core;

    public class ResilienceHandlerTests
    {
        private static ResilienceOptions DefaultsNoWait() => new()
        {
            Timeout = new TimeoutOptions { Overall = TimeSpan.FromSeconds(15) },
            Retry = new RetryOptions
            {
                Enabled = true,
                Attempts = 3,
                BaseDelay = TimeSpan.Zero, // no real waiting in tests
                Jitter = false,
                ApplyToPost = false,
                RetryOn500 = false,
                RespectRetryAfter = true,
                MaxRetryAfter = TimeSpan.FromSeconds(60)
            }
        };

        private static HttpMessageInvoker BuildPipeline(DelegatingHandler underTest, HttpMessageHandler inner)
        {
            underTest.InnerHandler = inner;
            return new HttpMessageInvoker(underTest, true);
        }

        private static HttpRequestMessage NewRequest(HttpMethod method, string uri = "https://example.test/")
            => new(method, uri);

        [Fact(DisplayName = "Retry: Applied with non-idempotent methods by default")]
        public async Task Get_503_then_200_is_retried()
        {
            // Arrange
            var options = new SinchOptions { Resilience = DefaultsNoWait() };
            var handler = new ResilienceHandler(new TestOptions<SinchOptions>(options));

            var inner = new SequencedHandler(
                SequencedHandler.Return(HttpStatusCode.ServiceUnavailable),
                SequencedHandler.Return(HttpStatusCode.OK)
            );

            using var invoker = BuildPipeline(handler, inner);
            using var req = NewRequest(HttpMethod.Get);

            // Act
            var resp = await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            inner.Calls.Should().Be(2, "one initial attempt + one retry");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Retry: Not applied with non-idempotent methods by default")]
        public async Task Post_503_is_not_retried_by_default()
        {
            // Arrange
            var options = new SinchOptions { Resilience = DefaultsNoWait() };
            var handler = new ResilienceHandler(new TestOptions<SinchOptions>(options));

            var inner = new SequencedHandler(
                SequencedHandler.Return(HttpStatusCode.ServiceUnavailable),
                SequencedHandler.Return(HttpStatusCode.OK) // would succeed if retried
            );

            using var invoker = BuildPipeline(handler, inner);
            using var req = NewRequest(HttpMethod.Post);

            // Act
            var resp = await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            inner.Calls.Should().Be(1, "POST is non-idempotent; retries off by default");
            resp.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact(DisplayName = "Retry: Applied with non-idempotent methods when enabled")]
        public async Task Post_503_is_retried_when_ApplyToPost_enabled()
        {
            // Arrange
            var ro = DefaultsNoWait();
            ro.Retry.ApplyToPost = true;

            var options = new SinchOptions { Resilience = ro };
            var handler = new ResilienceHandler(new TestOptions<SinchOptions>(options));

            var inner = new SequencedHandler(
                SequencedHandler.Return(HttpStatusCode.ServiceUnavailable),
                SequencedHandler.Return(HttpStatusCode.OK)
            );

            using var invoker = BuildPipeline(handler, inner);
            using var req = NewRequest(HttpMethod.Post);

            // Act
            var resp = await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            inner.Calls.Should().Be(2);
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Retry: Retry-After header is respected")]
        public async Task RetryAfter_header_triggers_retry_on_429()
        {
            // Arrange
            var options = new SinchOptions { Resilience = DefaultsNoWait() };
            var handler = new ResilienceHandler(new TestOptions<SinchOptions>(options));

            var inner = new SequencedHandler(
                SequencedHandler.Return(HttpStatusCode.TooManyRequests, resp =>
                {
                    resp.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(1));
                }),
                SequencedHandler.Return(HttpStatusCode.OK)
            );

            using var invoker = BuildPipeline(handler, inner);
            using var req = NewRequest(HttpMethod.Get);

            // Act
            var resp = await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            inner.Calls.Should().Be(2, "Retry-After present should cause a retry");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Retry: 500 is not retried by default")]
        public async Task Get_500_is_not_retried_by_default()
        {
            // Arrange
            var options = new SinchOptions { Resilience = DefaultsNoWait() };
            var handler = new ResilienceHandler(new TestOptions<SinchOptions>(options));

            var inner = new SequencedHandler(
                SequencedHandler.Return(HttpStatusCode.InternalServerError),
                SequencedHandler.Return(HttpStatusCode.OK)
            );

            using var invoker = BuildPipeline(handler, inner);
            using var req = NewRequest(HttpMethod.Get);

            // Act
            var resp = await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            inner.Calls.Should().Be(1, "500 is not retried unless RetryOn500=true");
            resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact(DisplayName = "Retry: 500 is retried when enabled")]
        public async Task Get_500_is_retried_when_RetryOn500_enabled()
        {
            // Arrange
            var ro = DefaultsNoWait();
            ro.Retry.RetryOn500 = true;

            var options = new SinchOptions { Resilience = ro };
            var handler = new ResilienceHandler(new TestOptions<SinchOptions>(options));

            var inner = new SequencedHandler(
                SequencedHandler.Return(HttpStatusCode.InternalServerError),
                SequencedHandler.Return(HttpStatusCode.OK)
            );

            using var invoker = BuildPipeline(handler, inner);
            using var req = NewRequest(HttpMethod.Get);

            // Act
            var resp = await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            inner.Calls.Should().Be(2);
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Timeout: Overall timeout triggers TimeoutRejectedException")]
        public async Task Overall_timeout_throws_Polly_timeout_rejection()
        {
            // Arrange
            var ro = DefaultsNoWait();
            ro.Timeout.Enabled = true;
            ro.Timeout.Overall = TimeSpan.FromMilliseconds(50);

            var options = new SinchOptions { Resilience = ro };
            var handler = new ResilienceHandler(new TestOptions<SinchOptions>(options));

            // inner delays longer than the overall timeout and HONORS cancellation
            var inner = new SequencedHandler(
                SequencedHandler.Delay(TimeSpan.FromMilliseconds(500))
            );

            using var invoker = BuildPipeline(handler, inner);
            using var req = NewRequest(HttpMethod.Get);

            // Act
            var act = async () => await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<TimeoutRejectedException>();
            inner.Calls.Should().Be(1);
        }

        [Fact(DisplayName = "Retry: Jitter off and zero base delay makes retries immediate")]
        public async Task Jitter_off_and_zero_base_delay_makes_retries_immediate()
        {
            // Arrange
            var ro = DefaultsNoWait();
            ro.Retry.Jitter = false;
            ro.Retry.BaseDelay = TimeSpan.Zero;

            var options = new SinchOptions { Resilience = ro };
            var handler = new ResilienceHandler(new TestOptions<SinchOptions>(options));
            var inner = new SequencedHandler(
                SequencedHandler.Return(HttpStatusCode.BadGateway), // 502 → retriable
                SequencedHandler.Return(HttpStatusCode.OK)
            );

            using var invoker = BuildPipeline(handler, inner);
            using var req = NewRequest(HttpMethod.Get);

            var before = DateTime.UtcNow;
            var resp = await invoker.SendAsync(req, CancellationToken.None);
            var after = DateTime.UtcNow;

            // Assert
            inner.Calls.Should().Be(2);
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            (after - before).Should().BeLessThan(TimeSpan.FromMilliseconds(50)); // "immediate-ish"
        }
    }
}