using GameService.Context;
using GameService.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PokerClassLibrary;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GameService.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly GameDbContext _dBcontext;
        readonly string JwtCryptoKey;

        public IdentityService(GameDbContext context, IConfiguration config)
        {
            _dBcontext = context;
            JwtCryptoKey = config.GetSection("CryptographicKey").Value;
        }
        

        public async Task<Player> Authenticate(string email, string password)
        {
            var user = await _dBcontext.Players.Include(user => user.Identity).SingleOrDefaultAsync(user => user.Email == email);

            // return null if user not found
            if (user == null)
                return null;

            var hashedPassword = Encryption.EncryptPassword(password, user.Identity.Hash);

            if (hashedPassword != user.Identity.Password) // Password doesn't match...
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtCryptoKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Issuer = "NetCorePoker",
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Identity.SessionToken = tokenHandler.WriteToken(token);

            _dBcontext.Players.Update(user);

            await _dBcontext.SaveChangesAsync();


            return user;
        }

        public Task<bool> IsAuthenticated(Guid playerId)
        {
            throw new NotImplementedException();
        }

        public Task Logout(Guid playerId)
        {
            throw new NotImplementedException();
        }
    }
}
