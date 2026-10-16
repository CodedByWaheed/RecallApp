using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_BL.Services.Security
{
    public class RefreshTokenService
    {
        public string GenerateToken()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        public byte[] HashToken(string token)
        {
            byte[] tokenBytes = Convert.FromBase64String(token);
            return SHA256.HashData(tokenBytes);
        }
    }
}
