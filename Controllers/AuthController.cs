using System.Security.Claims;
using FlowerShop.Api.Data;
using FlowerShop.Api.Dtos;
using FlowerShop.Api.Models;
using FlowerShop.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FlowerShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, TokenService tokens, IConfiguration config) : ControllerBase
{
    // PasswordHasher của ASP.NET Core: PBKDF2-HMAC-SHA512, muối ngẫu nhiên cho từng mật khẩu, hash có đánh số phiên bản.
    private static readonly PasswordHasher<User> Hasher = new();

    // Hash giả để đăng nhập với email không tồn tại cũng tốn thời gian kiểm tra như email có thật.
    private static readonly string DummyHash = Hasher.HashPassword(new User(), "dummy-password-1");

    /// <summary>POST /api/auth/register — tạo tài khoản, lưu vào DB và đăng nhập luôn (trả JWT).</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);

        if (await db.Users.AnyAsync(u => u.Email == email))
            return EmailTaken();

        var user = new User { Email = email, Role = RoleFor(email) };
        user.PasswordHash = Hasher.HashPassword(user, request.Password);
        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Hai người đăng ký cùng email cùng lúc: chỉ mục duy nhất của DB chặn người đến sau.
            return EmailTaken();
        }

        return StatusCode(StatusCodes.Status201Created, IssueToken(user));
    }

    /// <summary>POST /api/auth/login — truy vấn user theo email rồi kiểm tra mật khẩu với bản băm trong DB.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        var result = Hasher.VerifyHashedPassword(user ?? new User(), user?.PasswordHash ?? DummyHash, request.Password);
        if (user is null || result == PasswordVerificationResult.Failed)
        {
            // Một câu chung cho cả "sai email" và "sai mật khẩu" để không lộ email nào đã đăng ký.
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Đăng nhập thất bại",
                detail: "Email hoặc mật khẩu không đúng.");
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = Hasher.HashPassword(user, request.Password);

        var role = RoleFor(email);
        if (role == UserRoles.Admin && user.Role != UserRoles.Admin)
            user.Role = role;

        await db.SaveChangesAsync();
        return IssueToken(user);
    }

    /// <summary>GET /api/auth/me — front-end gọi khi mở lại trang để xác nhận token trong localStorage còn dùng được.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var idClaim = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var id))
            return Unauthorized();

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return user is null ? Unauthorized() : UserDto.From(user);
    }

    private AuthResponse IssueToken(User user)
    {
        var (token, expiresAt) = tokens.Create(user);
        return new AuthResponse(token, expiresAt, UserDto.From(user));
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>Email nằm trong cấu hình Auth:AdminEmails thì là Admin, còn lại là khách hàng.</summary>
    private string RoleFor(string email)
    {
        var admins = config.GetSection("Auth:AdminEmails").Get<string[]>() ?? [];
        return admins.Any(admin => string.Equals(admin.Trim(), email, StringComparison.OrdinalIgnoreCase))
            ? UserRoles.Admin
            : UserRoles.Customer;
    }

    private ObjectResult EmailTaken() => Problem(
        statusCode: StatusCodes.Status409Conflict,
        title: "Email đã được đăng ký",
        detail: "Email này đã có tài khoản. Hãy đăng nhập hoặc dùng email khác.");
}
