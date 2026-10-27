using FlowerShop.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Slug).HasMaxLength(60);
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.Property(c => c.Name).HasMaxLength(80);
            entity.Property(c => c.Description).HasMaxLength(300);
            entity.Property(c => c.PetalColor).HasMaxLength(7);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(120);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.Flowers).HasMaxLength(200);
            entity.Property(p => p.Price).HasPrecision(18, 0);
            entity.Property(p => p.PetalColor).HasMaxLength(7);

            // Không cho xóa danh mục còn sản phẩm.
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.CustomerName).HasMaxLength(100);
            entity.Property(o => o.Phone).HasMaxLength(20);
            entity.Property(o => o.Address).HasMaxLength(250);
            entity.Property(o => o.DeliverySlot).HasMaxLength(30);
            entity.Property(o => o.Message).HasMaxLength(500);

            // Lưu trạng thái dạng chữ ("New", "Completed"...) cho dễ đọc khi xem bảng.
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);

            // Code và IsFinal được tính từ thuộc tính khác, không phải cột trong bảng.
            entity.Ignore(o => o.Code);
            entity.Ignore(o => o.IsFinal);

            entity.HasIndex(o => o.Status);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.UnitPrice).HasPrecision(18, 0);

            // Xóa đơn thì xóa luôn các dòng hàng; không cho xóa sản phẩm đang nằm trong đơn.
            entity.HasOne<Order>()
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        SeedData.Apply(modelBuilder);
    }
}
