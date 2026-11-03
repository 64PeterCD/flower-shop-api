using System.ComponentModel.DataAnnotations;
using FlowerShop.Api.Models;

namespace FlowerShop.Api.Dtos;

public record UserDto(int Id, string Email, string Role)
{
    public static UserDto From(User user) => new(user.Id, user.Email, user.Role);
}

/// <summary>Kết quả đăng ký / đăng nhập: JWT để gắn vào các request sau + thông tin user.</summary>
public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

public class RegisterRequest
{
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [StringLength(254, ErrorMessage = "Email tối đa 254 ký tự.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 100 ký tự.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Mật khẩu cần có ít nhất một chữ cái và một chữ số.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Đăng nhập chỉ kiểm tra "có nhập" — không tiết lộ quy tắc mật khẩu và không báo email nào có tài khoản.</summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
}
