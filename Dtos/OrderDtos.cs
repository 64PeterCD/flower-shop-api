using System.ComponentModel.DataAnnotations;
using FlowerShop.Api.Models;

namespace FlowerShop.Api.Dtos;

public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);

/// <summary>Đơn hàng trả về cho front-end (kèm tổng tiền và các dòng hàng).</summary>
public record OrderDto(
    int Id,
    string Code,
    string CustomerName,
    string Phone,
    string Address,
    DateOnly DeliveryDate,
    string DeliverySlot,
    string? Message,
    OrderStatus Status,
    decimal Total,
    DateTime CreatedAt,
    List<OrderItemDto> Items)
{
    public static OrderDto From(Order order) => new(
        order.Id,
        order.Code,
        order.CustomerName,
        order.Phone,
        order.Address,
        order.DeliveryDate,
        order.DeliverySlot,
        order.Message,
        order.Status,
        order.Items.Sum(item => item.Quantity * item.UnitPrice),
        order.CreatedAt,
        order.Items.Select(item => new OrderItemDto(item.ProductId, item.Product.Name, item.Quantity, item.UnitPrice)).ToList());
}

public class CreateOrderItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã sản phẩm không hợp lệ.")]
    public int ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000.")]
    public int Quantity { get; set; }
}

/// <summary>Khách đặt hoa (POST /api/orders).</summary>
public class CreateOrderRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hoa.")]
    [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự.")]
    public string Address { get; set; } = string.Empty;

    public DateOnly DeliveryDate { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn khung giờ giao.")]
    public string DeliverySlot { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Lời nhắn tối đa 500 ký tự.")]
    public string? Message { get; set; }

    [Required, MinLength(1, ErrorMessage = "Đơn hàng cần ít nhất một sản phẩm.")]
    public List<CreateOrderItemRequest> Items { get; set; } = [];
}

/// <summary>Chủ shop cập nhật đơn (PUT /api/orders/{id}); gửi đủ các trường, không chỉ phần thay đổi.</summary>
public class UpdateOrderRequest
{
    [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
    public OrderStatus? Status { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hoa.")]
    [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự.")]
    public string Address { get; set; } = string.Empty;

    public DateOnly DeliveryDate { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn khung giờ giao.")]
    public string DeliverySlot { get; set; } = string.Empty;
}
