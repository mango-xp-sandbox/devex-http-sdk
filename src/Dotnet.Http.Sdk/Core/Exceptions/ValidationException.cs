namespace Dotnet.Http.Sdk.Core.Exceptions
{
    public sealed class ValidationException(string message, int statusCode, string? errorCode = null, Exception? innerException = null)
        : SinchException(message, statusCode, errorCode, innerException);
}