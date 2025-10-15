namespace Dotnet.Http.Sdk.SignatureVerifier
{
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Extensions.Options;
    using Public;

    internal sealed class SinchSignatureVerifier(IOptions<SignatureVerificationOptions> opts) : ISinchSignatureVerifier
    {
        private readonly SignatureVerificationOptions _opts = opts.Value;

        public bool Verify(string? authorizationHeader, ReadOnlySpan<byte> body)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader)) return false;

            var idx = authorizationHeader.IndexOf(' '); // scheme separator
            if (idx <= 0) return false;

            // Scheme must match
            var scheme = authorizationHeader[..idx].Trim();
            if (!scheme.Equals(_opts.Scheme, StringComparison.OrdinalIgnoreCase)) return false;

            // Rest of header is the provided signature
            var provided = authorizationHeader[(idx + 1)..].Trim();
            if (provided.Length == 0) return false;

            // Compute HMAC
            var key = Encoding.UTF8.GetBytes(_opts.Secret);
            using var hmac = new HMACSHA256(key);
            var computed = hmac.ComputeHash(body.ToArray()); // HMAC APIs need byte[]; payload is small here.

            var computedHex = Convert.ToHexString(computed); // uppercase hex

            return FixedEqualsHex(computedHex, provided);
        }

        private static bool FixedEqualsHex(string aLower, string bAnyCase)
        {
            // normalize b to lowercase efficiently
            var bLower = bAnyCase.ToUpperInvariant();
            var aBytes = Encoding.ASCII.GetBytes(aLower);
            var bBytes = Encoding.ASCII.GetBytes(bLower);
            return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
        }
    }
}