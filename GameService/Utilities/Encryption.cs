using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameService.Utilities
{
    public static class Encryption
    {
        public static string GenerateHash(string email, Guid userId)
        {
            var salt = userId.ToString() + Guid.NewGuid().ToString();

            string token = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: email,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 1000,
                numBytesRequested: 128 / 8));

            return UrlEncode(token);
        }

        internal static string UrlEncode(string base64Encoded) => Base64UrlEncoder.Encode(base64Encoded);

        public static string EncryptPassword(string password, string hash)
        {
            string encrypted = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.UTF8.GetBytes(hash),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 1000,
                numBytesRequested: 128 / 8));

            return encrypted;
        }
    }
}
