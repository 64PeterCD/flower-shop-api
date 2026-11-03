using System.Security.Claims;
using System.Text;
using FlowerShop.Api.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FlowerShop.Api.Services;

/// <summary>Cấu hình JWT đọc từ mục "Jwt" (appsettings.json; khóa bí mật đặt bằng user-secrets).</summary>
public record JwtSettings(string Issuer, string Audience, string Key, int ExpiresMinutes)
{
    public SymmetricSecurityKey SigningKey => new(Encoding.UTF8.GetBytes(Key));

    public static JwtSettings From(IConfiguration config)
    {
        var section = config.GetSection("Jwt");
        var key = section["Key"];

        if (string.IsNullOrWhiteSpace(key) || key.StartsWith("CHANGE_ME", StringComparison.Ordinal) || key.Length < 32)
        {
            throw new InvalidOperationException(
                "Chưa cấu hình Jwt:Key (cần ít nhất 32 ký tự). Chạy: dotnet user-secrets set \"Jwt:Key\" \"<chuỗi ngẫu nhiên dài>\"");
        }

        return new JwtSettings(
            section["Issuer"] ?? "FlowerShop.Api",
            section["Audience"] ?? "FlowerShop.Web",
            key,
            section.GetValue("ExpiresMinutes", 120));
    }
}

public class TokenService(IConfiguration config)
{
    /// <summary>Tên claim chứa vai trò; JwtBearer được cấu hình dùng cùng tên này để [Authorize(Roles = ...)] hoạt động.</summary>
    public const string RoleClaim = "role";

    public (string Token, DateTime ExpiresAt) Create(User user)
    {
        var settings = JwtSettings.From(config);
        var expires = DateTime.UtcNow.AddMinutes(settings.ExpiresMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Expires = expires,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(RoleClaim, user.Role),
            ]),
            SigningCredentials = new SigningCredentials(settings.SigningKey, SecurityAlgorithms.HmacSha256),
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }
}
