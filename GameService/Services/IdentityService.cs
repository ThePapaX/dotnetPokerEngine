using GameService.Context;
using GameService.Utilities;
using Microsoft.IdentityModel.Tokens;
using PokerClassLibrary;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GameService.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly GameDbContext _context;
        private readonly string _jwtCryptoKey;

        public IdentityService(GameDbContext context, string securityKey)
        {
            _context = context;
        }

        async Task<Player> IIdentityService.Authenticate(string username, string password)
        {
            var user = await _context.Players.Include(user => user.Identity).SingleOrDefaultAsync(user => user.UserName == username);

            // return null if user not found
            if (user == null)
                return null;

            var hashedPassword = Encryption.EncryptPassword(password, user.Identity.Hash);

            if (hashedPassword != user.Identity.Password) // Password doesn't match...
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtCryptoKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Identity.SessionToken = tokenHandler.WriteToken(token);

            _context.Players.Update(user);

            await _context.SaveChangesAsync();


            return user;
        }

        Task<bool> IIdentityService.IsAuthenticated(Guid playerId)
        {
            throw new NotImplementedException();
        }

        Task IIdentityService.Logout(Guid playerId)
        {
            throw new NotImplementedException();
        }
    }
}
