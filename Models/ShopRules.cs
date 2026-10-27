namespace FlowerShop.Api.Models;

/// <summary>Quy tắc nghiệp vụ dùng chung của shop.</summary>
public static class ShopRules
{
    /// <summary>Các khung giờ giao hoa hợp lệ (khớp danh sách ở front-end).</summary>
    public static readonly string[] DeliverySlots =
    [
        "08:00 – 10:00",
        "10:00 – 12:00",
        "13:00 – 15:00",
        "15:00 – 17:00",
        "17:00 – 19:00",
        "19:00 – 21:00",
    ];

    /// <summary>Ngày hôm nay theo giờ Việt Nam (UTC+7, không có giờ mùa hè).</summary>
    public static DateOnly TodayInVietnam => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
}
