using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Helpers
{
    public static class JwtTokenGenerator
    {
        //    public static string GenerateToken(AppUser user, IConfiguration config)
        //    {
        //        var jwtSettings = config.GetSection("JwtSettings");

        //        var claims = new[]
        //        {
        //            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        //            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        //            new Claim(ClaimTypes.Role, user.Role),
        //            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //        };

        //        var keyString = jwtSettings["Key"];
        //        if (string.IsNullOrEmpty(keyString))
        //            throw new InvalidOperationException("JWT key is missing in configuration.");

        //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        //        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //        var expirationMinutesString = jwtSettings["ExpirationMinutes"];
        //        if (string.IsNullOrEmpty(expirationMinutesString))
        //            throw new InvalidOperationException("JWT expiration minutes is missing in configuration.");

        //        var token = new JwtSecurityToken(
        //            issuer: jwtSettings["Issuer"],
        //            audience: jwtSettings["Audience"],
        //            claims: claims,
        //            expires: DateTime.UtcNow.AddMinutes(double.Parse(expirationMinutesString)),
        //            signingCredentials: creds
        //        );

        //        return new JwtSecurityTokenHandler().WriteToken(token);
        //    }
    }
}