namespace Dotnet.Http.Sdk.Core.Exceptions
{
    public abstract class SinchException(string message, int statusCode, string? errorCode = null, Exception? inner = null)
        : Exception(message, inner)
    {
        public int StatusCode { get; } = statusCode;
        public string? ErrorCode { get; } = errorCode;
    }
}