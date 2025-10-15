namespace Dotnet.Http.Sdk.UnitTests.Core.Http
{
    using System.Net;

    internal sealed class SequencedHandler(params Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>[] steps) : DelegatingHandler
    {
        private readonly Queue<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>> _steps = new(steps);

        public int Calls { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            if (_steps.Count == 0) throw new InvalidOperationException("No more steps configured in SequencedHandler.");

            var step = _steps.Dequeue();
            return await step(request, cancellationToken);
        }

        public static Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Return(HttpStatusCode code, Action<HttpResponseMessage>? mutate = null)
            => (req, _) =>
            {
                var resp = new HttpResponseMessage(code)
                {
                    RequestMessage = req
                };
                mutate?.Invoke(resp);
                return Task.FromResult(resp);
            };

        public static Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Delay(TimeSpan delay, HttpStatusCode code = HttpStatusCode.OK)
            => async (req, ct) =>
            {
                // honor cancellation for optimistic timeout
                await Task.Delay(delay, ct);
                return new HttpResponseMessage(code) { RequestMessage = req };
            };
    }
}