namespace Dotnet.Http.Sdk.UnitTests.SignatureVerifier
{
    using System.Text;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Public;
    using Sdk.SignatureVerifier;

    public class SinchSignatureVerifierTests
    {
        [Theory(DisplayName = "Verification: Validate valid signatures")]
        [InlineData("hello", "mySecret", "Signature 7155DA4425DFA360FDF653DF6A13ADA9B7E804AB2A5892EA36AB84BFF7FEDAEE")]
        [InlineData("mysuperduperbody", "mySecret", "Signature FF77A69DC96FD52587A587309DAF141D26DD8DAB1F4E85F22C59D6E87F751C0F")]
        public void Verify_valid(string body, string secret, string header)
        {
            var svc = Make(secret);
            svc.Verify(header, Encoding.UTF8.GetBytes(body)).Should().BeTrue();
        }

        [Fact(DisplayName = "Verification: Reject invalid signature")]
        public void Verify_wrong_scheme() =>
            Make().Verify("Bearer deadbeef", "a"u8.ToArray()).Should().BeFalse();

        // helper
        private static ISinchSignatureVerifier Make(string secret = "mySecret") =>
            new SinchSignatureVerifier(Options.Create(new SignatureVerificationOptions { Secret = secret }));
    }
}