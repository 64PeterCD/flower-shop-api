namespace FlowerShop.Api.Models;

public static class UserRoles
{
    public const string Customer = "Customer";
    public const string Admin = "Admin";
}

public class User
{
    public int Id { get; set; }

    /// <summary>Email đăng nhập, luôn lưu dạng chữ thường để so sánh không phân biệt hoa/thường.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Mật khẩu đã băm (PBKDF2 + salt ngẫu nhiên); không bao giờ lưu mật khẩu gốc.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = UserRoles.Customer;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
