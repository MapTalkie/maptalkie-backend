using System;

namespace MapTalkie.Services.TokenService
{
    public class JwtTokenResult
    {
        public JwtTokenResult(string fullToken, DateTime expiresAt)
        {
            FullToken = fullToken;
            if (fullToken.Split(".").Length != 3)
                throw new FormatException("Invalid JWT token");
            ExpiresAt = expiresAt;
        }

        public string FullToken { get; }
        public DateTime ExpiresAt { get; set; }

        public string Signature => FullToken[(FullToken.LastIndexOf(".", StringComparison.Ordinal) + 1)..];

        public string TokenBase => FullToken[..FullToken.LastIndexOf(".", StringComparison.Ordinal)];
    }
}