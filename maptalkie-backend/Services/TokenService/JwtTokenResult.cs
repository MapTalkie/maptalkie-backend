using System;
using System.IdentityModel.Tokens.Jwt;

namespace MapTalkie.Services.TokenService
{
    public class JwtTokenResult
    {
        public JwtTokenResult(string fullToken)
        {
            FullToken = fullToken;
            if (fullToken.Split(".").Length != 3)
                throw new FormatException("Invalid JWT token");
        }
        
        public string FullToken { get; }

        public string Signature => FullToken[(FullToken.LastIndexOf(".", StringComparison.Ordinal)+1)..];

        public string TokenBase => FullToken[..FullToken.LastIndexOf(".", StringComparison.Ordinal)];
    }
}