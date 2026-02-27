using KesarPremium.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Services.AuthServices
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config) => _config = config;

        public string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:AccessTokenExpiryMinutes"]!));

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Name,           user.FullName),
            new Claim(ClaimTypes.Role,           user.Role?.RoleName ?? "User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public int? GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token)) return null;
            var jwt = handler.ReadJwtToken(token);
            var idClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }

}
