namespace Dotnet.Http.Sdk.Core
{
    using System.Net;
    using System.Text.Json;
    using Exceptions;
    using Polly.Timeout;

    internal static class ErrorMapper
    {
        /// <summary>
        /// Maps a non-successful HTTP response to a canonical SDK exception.
        /// </summary>
        public static Exception Map(HttpStatusCode statusCode, string? responseBody)
        {
            var message = ParseMessageAndCode(responseBody);

            return statusCode switch
            {
                HttpStatusCode.BadRequest => new ValidationException(message ?? "Bad request.", (int)statusCode),
                HttpStatusCode.Unauthorized => new UnauthorizedException(message ?? "Unauthorized.", (int)statusCode),
                HttpStatusCode.NotFound => new NotFoundException(message ?? "Resource not found.", (int)statusCode),

                HttpStatusCode.Forbidden => new UnauthorizedException(message ?? "Forbidden.", (int)statusCode),

                // Too Many Requests / Service Unavailable etc.
                HttpStatusCode.RequestTimeout => new InternalServerException(message ?? "Request timed out.", (int)statusCode),
                _ when (int)statusCode >= 500 => new InternalServerException(message ?? "Server error.", (int)statusCode),

                // Everything else falls back to internal error to avoid leaking transport details
                _ => new InternalServerException(message ?? $"HTTP {(int)statusCode}.", (int)statusCode)
            };
        }

        /// <summary>
        /// Maps transport-level exceptions (timeouts/cancellation/network) to canonical SDK exceptions.
        /// Call this from HttpGateway catch blocks.
        /// </summary>
        public static Exception MapTransport(Exception ex)
        {
            // Polly timeout (optimistic)
            if (ex is TimeoutRejectedException) return new InternalServerException("Operation timed out.", 504);

            // HttpClient timeouts/cancellation
            if (ex is TaskCanceledException or OperationCanceledException) return new InternalServerException("Operation canceled or timed out.", 499); // non-standard but informative

            if (ex is HttpRequestException hre) return new InternalServerException(hre.Message, 503, null, hre);

            if (ex is JsonException je) return new InternalServerException($"Deserialization error: {je.Message}", 500, null, je);

            // Fallback
            return new InternalServerException(ex.Message, 500, null, ex);
        }

        /// <summary>
        /// Extracts a human message + vendor code from body, supporting RFC7807 and common ad-hoc formats.
        /// </summary>
        private static string? ParseMessageAndCode(string? body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;

            // Sample return of error body
            var trimmed = body.Length > 512 ? body[..512] + "…" : body;
            return trimmed;
        }
    }
}