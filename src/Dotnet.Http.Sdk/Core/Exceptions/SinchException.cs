namespace Dotnet.Http.Sdk.Core.Exceptions
{
    public abstract class SinchException : Exception
    {
        public int StatusCode { get; }
        public string? ErrorCode { get; }
    }
}