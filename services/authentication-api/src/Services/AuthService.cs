using AuthenticationApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationApi.Repositories;

namespace AuthenticationApi.Services
{
    public class AuthService
    {
        private readonly JwtSettings _jwt;
        private readonly UserRepository _repo;
        private readonly IConfiguration _configuration;

        public AuthService(IOptions<JwtSettings> jwt, UserRepository repo, IConfiguration configuration)
        {
            _jwt = jwt.Value;
            _repo = repo;
            _configuration = configuration;
        }

        public string GenerateTokenForUser(User user)
        {
            return GenerateToken(user);
        }

        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim("sub", user.Id),
                new Claim("role", user.Role),
            };

            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT Secret Key is not set in configuration.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}