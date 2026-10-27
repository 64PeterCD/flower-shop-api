namespace FlowerShop.Api.Models;

public enum OrderStatus
{
    New,
    Preparing,
    Delivering,
    Completed,
    Cancelled,
}

public class Order
{
    public int Id { get; set; }

    /// <summary>Mã đơn hiển thị cho khách, suy ra từ Id nên không lưu trong database.</summary>
    public string Code => $"HOA-{1000 + Id}";

    public string CustomerName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public DateOnly DeliveryDate { get; set; }

    public string DeliverySlot { get; set; } = string.Empty;

    public string? Message { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.New;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = [];

    public bool IsFinal => Status is OrderStatus.Completed or OrderStatus.Cancelled;
}

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    /// <summary>Giá tại thời điểm đặt, để đổi giá sản phẩm sau này không làm sai đơn cũ.</summary>
    public decimal UnitPrice { get; set; }
}
