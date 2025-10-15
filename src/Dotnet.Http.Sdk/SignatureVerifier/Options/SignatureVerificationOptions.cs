namespace Dotnet.Http.Sdk.SignatureVerifier
{
    public sealed class SignatureVerificationOptions
    {
        public string Secret { get; set; } = "mySecret";
        public string Scheme { get; set; } = "Signature";
    }
}