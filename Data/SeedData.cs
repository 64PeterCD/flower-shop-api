using FlowerShop.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Api.Data;

/// <summary>Dữ liệu mẫu, được ghi vào database cùng migration (HasData).</summary>
public static class SeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Slug = "hoa-bo", Name = "Hoa bó", Description = "Bó hoa cho sinh nhật, kỷ niệm và những lời muốn nói.", PetalColor = "#d9506f" },
            new Category { Id = 2, Slug = "gio-hop", Name = "Giỏ & hộp hoa", Description = "Hoa cắm sẵn, đặt lên bàn là đẹp.", PetalColor = "#8fa8e8" },
            new Category { Id = 3, Slug = "khai-truong", Name = "Hoa khai trương", Description = "Kệ hoa chúc mừng, nổi bật trước cửa hàng.", PetalColor = "#ee8a33" },
            new Category { Id = 4, Slug = "hoa-cuoi", Name = "Hoa cưới", Description = "Hoa cầm tay và phụ kiện cho ngày trọng đại.", PetalColor = "#e9c9d2" });

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, CategoryId = 1, Name = "Bó hồng đỏ Tình Yêu", Price = 450000, Stock = 12, PetalColor = "#c2274b", Featured = true, Flowers = "Hồng đỏ Ecuador, baby trắng, lá bạc", Description = "Mười một bông hồng đỏ gói giấy kraft, thích hợp tặng người thương nhân dịp kỷ niệm hoặc Valentine." },
            new Product { Id = 2, CategoryId = 1, Name = "Bó hướng dương Nắng Sớm", Price = 380000, Stock = 8, PetalColor = "#f2b01e", Featured = true, Flowers = "Hướng dương, cúc tana, lá bạc", Description = "Sắc vàng rực rỡ mang năng lượng tích cực, hợp tặng bạn bè, đồng nghiệp hoặc chúc mừng tốt nghiệp." },
            new Product { Id = 3, CategoryId = 1, Name = "Bó tulip Hồng Pastel", Price = 520000, Stock = 4, PetalColor = "#f09ab5", Featured = true, Flowers = "Tulip hồng, hoa mận, lá eucalyptus", Description = "Tulip nhập theo mùa, tông hồng phấn dịu dàng. Số lượng có hạn mỗi tuần." },
            new Product { Id = 4, CategoryId = 2, Name = "Giỏ hoa Cẩm Tú Cầu", Price = 650000, Stock = 6, PetalColor = "#8fa8e8", Featured = true, Flowers = "Cẩm tú cầu xanh, hồng phấn, cúc mẫu đơn", Description = "Giỏ mây cắm sẵn với cẩm tú cầu đầy đặn, thêm thiệp viết tay theo yêu cầu." },
            new Product { Id = 5, CategoryId = 2, Name = "Hộp hoa Mùa Xuân", Price = 590000, Stock = 3, PetalColor = "#e58bb8", Featured = false, Flowers = "Hồng spray, cát tường, hoa lan tỏi", Description = "Hộp tròn cắm hoa tươi nhiều tầng màu, gọn nhẹ, dễ mang đi tặng." },
            new Product { Id = 6, CategoryId = 3, Name = "Kệ hoa Phát Tài", Price = 1200000, Stock = 5, PetalColor = "#ee8a33", Featured = false, Flowers = "Đồng tiền cam, hồng vàng, lan vũ nữ", Description = "Kệ hoa hai tầng cao 1,6 m chúc mừng khai trương. Giao và dựng tại cửa hàng." },
            new Product { Id = 7, CategoryId = 3, Name = "Kệ hoa Thành Công", Price = 1500000, Stock = 2, PetalColor = "#d63f3f", Featured = false, Flowers = "Hồng đỏ, lay ơn, hướng dương, lá phát tài", Description = "Kệ hoa lớn tông đỏ – vàng, cao 1,8 m, phù hợp lễ khai trương, kỷ niệm thành lập." },
            new Product { Id = 8, CategoryId = 4, Name = "Hoa cầm tay Thiên Thần", Price = 850000, Stock = 7, PetalColor = "#efd9de", Featured = false, Flowers = "Hồng trắng, lan Mokara, baby, lá bạc", Description = "Bó hoa cưới tông trắng kem, kèm hoa cài áo cho chú rể. Hẹn thử trước 3 ngày." });

        modelBuilder.Entity<Order>().HasData(
            NewOrder(1, "Hoàng Đức Long", "0900 000 007", "15 Trần Hưng Đạo, Quận 5", new DateOnly(2026, 10, 25), "08:00 – 10:00", OrderStatus.Completed, 23),
            NewOrder(2, "Đặng Khánh Linh", "0900 000 006", "7 Võ Văn Tần, Quận 3", new DateOnly(2026, 10, 25), "19:00 – 21:00", OrderStatus.Cancelled, 23),
            NewOrder(3, "Võ Anh Tuấn", "0900 000 005", "102 Nguyễn Thị Minh Khai, Quận 3", new DateOnly(2026, 10, 26), "13:00 – 15:00", OrderStatus.Completed, 24),
            NewOrder(4, "Phạm Gia Hân", "0900 000 004", "33 Lý Tự Trọng, Quận 1", new DateOnly(2026, 10, 26), "08:00 – 10:00", OrderStatus.Completed, 24),
            NewOrder(5, "Lê Bảo Châu", "0900 000 003", "9 Pasteur, Quận 1", new DateOnly(2026, 10, 27), "17:00 – 19:00", OrderStatus.Preparing, 26),
            NewOrder(6, "Trần Minh Quân", "0900 000 002", "250 Điện Biên Phủ, Quận Bình Thạnh", new DateOnly(2026, 10, 27), "10:00 – 12:00", OrderStatus.Delivering, 26),
            NewOrder(7, "Nguyễn Thu Hà", "0900 000 001", "21 Nguyễn Du, Quận 1", new DateOnly(2026, 10, 27), "15:00 – 17:00", OrderStatus.New, 27));

        modelBuilder.Entity<OrderItem>().HasData(
            NewItem(1, 1, 7, 1, 1500000),
            NewItem(2, 2, 5, 1, 590000),
            NewItem(3, 3, 2, 3, 380000),
            NewItem(4, 4, 8, 1, 850000),
            NewItem(5, 5, 3, 1, 520000),
            NewItem(6, 5, 4, 1, 650000),
            NewItem(7, 6, 6, 2, 1200000),
            NewItem(8, 7, 1, 1, 450000));
    }

    private static Order NewOrder(int id, string customer, string phone, string address, DateOnly date, string slot, OrderStatus status, int dayCreated) => new()
    {
        Id = id,
        CustomerName = customer,
        Phone = phone,
        Address = address,
        DeliveryDate = date,
        DeliverySlot = slot,
        Status = status,
        CreatedAt = new DateTime(2026, 10, dayCreated, 3, 0, 0, DateTimeKind.Utc),
    };

    private static OrderItem NewItem(int id, int orderId, int productId, int quantity, decimal unitPrice) => new()
    {
        Id = id,
        OrderId = orderId,
        ProductId = productId,
        Quantity = quantity,
        UnitPrice = unitPrice,
    };
}
