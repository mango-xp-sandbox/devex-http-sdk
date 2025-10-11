namespace Dotnet.Http.Sdk.Core.Exceptions
{
    public sealed class NotFoundException(string message, int statusCode, string? errorCode = null, Exception? innerException = null)
        : SinchException(message, statusCode, errorCode, innerException);
}