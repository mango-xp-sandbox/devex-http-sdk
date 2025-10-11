namespace Dotnet.Http.Sdk.Core.Exceptions
{
    public sealed class InternalServerException(string message, int statusCode, string? errorCode = null, Exception? innerException = null)
        : SinchException(message, statusCode, errorCode, innerException);
}