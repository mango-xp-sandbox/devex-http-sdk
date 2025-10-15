namespace Dotnet.Http.Sdk.Core
{
    using System.Net;
    using Microsoft.Extensions.Options;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;
    using Polly.Timeout;

    internal sealed class ResilienceHandler(IOptions<SinchOptions> options) : DelegatingHandler
    {
        private AsyncTimeoutPolicy<HttpResponseMessage>? _timeoutPolicy;
        private AsyncRetryPolicy<HttpResponseMessage>? _retryPolicy;
        private IAsyncPolicy<HttpResponseMessage>? _composed;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            _composed ??= BuildPolicy(options.Value.Resilience);

            return await _composed.ExecuteAsync(
                token => base.SendAsync(request, token),
                ct);
        }

        private IAsyncPolicy<HttpResponseMessage> BuildPolicy(ResilienceOptions ro)
        {
            var policies = new List<IAsyncPolicy<HttpResponseMessage>>();

            // Overall timeout (optimistic)
            if (ro.Timeout is { Enabled: true, Overall: { } overall })
            {
                _timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(overall, TimeoutStrategy.Optimistic);
                policies.Add(_timeoutPolicy);
            }

            // Retries (idempotent by default)
            if (ro.Retry.Enabled)
            {
                _retryPolicy = BuildRetryPolicy(ro.Retry);
                policies.Add(_retryPolicy);
            }

            return policies.Count switch
            {
                0 => Policy.NoOpAsync<HttpResponseMessage>(),
                1 => policies[0],
                _ => Policy.WrapAsync(policies.ToArray())
            };
        }

        private AsyncRetryPolicy<HttpResponseMessage> BuildRetryPolicy(RetryOptions r)
        {
            // Backoff sequence
            var delays = r.Jitter ? Backoff.DecorrelatedJitterBackoffV2(r.BaseDelay, r.Attempts) : Enumerable.Repeat(r.BaseDelay, r.Attempts);

            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>() // network timeouts can surface here
                .OrResult(ShouldRetryHttp(r))
                .WaitAndRetryAsync(
                    delays,
                    async (_, _, _, _) =>
                    {
                        // If retry policy needs heavy logging / telemetry, we'd do it here (or add extension point to hook in)
                        await Task.CompletedTask;
                    });
        }

        // Build a predicate that encapsulates idempotency + status rules
        private Func<HttpResponseMessage, bool> ShouldRetryHttp(RetryOptions r) => response =>
        {
            var req = response.RequestMessage;
            if (req is null) return false;

            // Idempotency gate
            var method = req.Method;
            var isIdempotent = method == HttpMethod.Get || method == HttpMethod.Head || method == HttpMethod.Options;
            if (!isIdempotent && !(r.ApplyToPost && method == HttpMethod.Post)) return false;

            // Status code families
            var code = (int)response.StatusCode;
            if (code is 408 or 429 or 502 or 503 or 504) return true;
            if (r.RetryOn500 && response.StatusCode == HttpStatusCode.InternalServerError) return true;

            // Retry-After honoring here; for assessment purposes we just honor it logically
            if (r.RespectRetryAfter && HasRetryAfter(response, out _)) return true;

            return false;
        };

        // Helper for Retry-After evaluation
        private static bool HasRetryAfter(HttpResponseMessage r, out TimeSpan delay)
        {
            delay = TimeSpan.Zero;
            var ra = r.Headers?.RetryAfter;
            if (ra == null) return false;

            if (ra.Delta is { } d)
            {
                delay = d;
                return true;
            }

            if (ra.Date is { } date)
            {
                delay = date - DateTimeOffset.UtcNow;
                return delay > TimeSpan.Zero;
            }

            return false;
        }
    }
}