using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthenticationApi.Models;
using AuthenticationApi.Repositories;

namespace AuthenticationApi.Services
{
    public class AuthService
    {
        private readonly JwtSettings _jwt;
        private readonly UserRepository _repo;

        public AuthService(IOptions<JwtSettings> jwt, UserRepository repo)
        {
            _jwt = jwt.Value;
            _repo = repo;
        }

        public async Task<string?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _repo.GetUserByUsernameAsync(username);
            if (user == null || !PasswordService.VerifyPassword(password, user.Password))
                return null;

            return GenerateToken(user);
        }


        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}