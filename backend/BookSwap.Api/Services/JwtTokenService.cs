using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookSwap.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace BookSwap.Api.Services;

public sealed class JwtTokenService(IConfiguration configuration, UserManager<AppUser> userManager)
{
    public async Task<(string Token, DateTime ExpiresAt)> CreateAsync(AppUser user)
    {
        var section = configuration.GetSection("Jwt");
        var expiresAt = DateTime.UtcNow.AddMinutes(section.GetValue<int>("ExpiresMinutes", 720));
        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(section["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: section["Issuer"],
            audience: section["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
