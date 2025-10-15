namespace Dotnet.Http.Sdk.Public
{
    public interface ISinchSignatureVerifier
    {
        bool Verify(string? authorizationHeader, ReadOnlySpan<byte> body);
    }
}