using System.Text.RegularExpressions;
using FlowerShop.Api.Data;
using FlowerShop.Api.Dtos;
using FlowerShop.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class OrdersController(AppDbContext db) : ControllerBase
{
    // 10 chữ số bắt đầu bằng 0 (hoặc +84 rồi 9 chữ số), cho phép khoảng trắng / dấu chấm.
    [GeneratedRegex(@"^(0|\+84)\d{9}$")]
    private static partial Regex PhonePattern();

    /// <summary>GET /api/orders?status=New — danh sách đơn, mới nhất trước.</summary>
    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetAll([FromQuery] OrderStatus? status)
    {
        var query = db.Orders.AsNoTracking().Include(o => o.Items).ThenInclude(i => i.Product).AsQueryable();

        if (status is not null)
            query = query.Where(o => o.Status == status);

        var orders = await query.OrderByDescending(o => o.Id).ToListAsync();
        return orders.Select(OrderDto.From).ToList();
    }

    /// <summary>GET /api/orders/{id} — chi tiết một đơn.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await db.Orders.AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order is null ? OrderNotFound(id) : OrderDto.From(order);
    }

    /// <summary>POST /api/orders — khách đặt hoa: kiểm tra dữ liệu, trừ tồn kho và lưu đơn trong một transaction.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request)
    {
        ValidatePhone(request.Phone);
        ValidateDelivery(request.DeliveryDate, request.DeliverySlot, checkPast: true);

        // Gộp các dòng trùng sản phẩm để trừ kho một lần.
        var lines = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => (ProductId: group.Key, Quantity: group.Sum(item => item.Quantity)))
            .ToList();

        var ids = lines.Select(line => line.ProductId).ToList();
        var products = await db.Products.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        foreach (var line in lines.Where(line => !products.ContainsKey(line.ProductId)))
            ModelState.AddModelError(nameof(request.Items), $"Sản phẩm mã {line.ProductId} không tồn tại.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        await using var transaction = await db.Database.BeginTransactionAsync();

        var order = new Order
        {
            CustomerName = request.CustomerName.Trim(),
            Phone = request.Phone.Trim(),
            Address = request.Address.Trim(),
            DeliveryDate = request.DeliveryDate,
            DeliverySlot = request.DeliverySlot,
            Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
        };

        foreach (var (productId, quantity) in lines)
        {
            var product = products[productId];

            // Trừ kho bằng một câu UPDATE có điều kiện: hai khách đặt cùng lúc không thể làm kho âm.
            var updated = await db.Products
                .Where(p => p.Id == productId && p.Stock >= quantity)
                .ExecuteUpdateAsync(set => set.SetProperty(p => p.Stock, p => p.Stock - quantity));

            if (updated == 0)
            {
                // Thoát mà chưa Commit -> transaction tự rollback, các sản phẩm đã trừ trước đó được hoàn lại.
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Không đủ hàng",
                    detail: $"Sản phẩm \"{product.Name}\" không còn đủ {quantity} món trong kho.");
            }

            order.Items.Add(new OrderItem { Product = product, Quantity = quantity, UnitPrice = product.Price });
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, OrderDto.From(order));
    }

    /// <summary>PUT /api/orders/{id} — đổi trạng thái / địa chỉ / lịch giao. Đơn đã hoàn thành hoặc đã hủy thì không sửa nữa.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<OrderDto>> Update(int id, UpdateOrderRequest request)
    {
        var order = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
            return OrderNotFound(id);

        if (order.IsFinal)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Không thể sửa đơn",
                detail: $"Đơn {order.Code} đã ở trạng thái kết thúc nên không thay đổi được nữa.");
        }

        // Chỉ kiểm tra "không ở quá khứ" khi ngày giao bị đổi, tránh chặn việc đổi trạng thái đơn đã trễ hẹn.
        ValidateDelivery(request.DeliveryDate, request.DeliverySlot, checkPast: request.DeliveryDate != order.DeliveryDate);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        await using var transaction = await db.Database.BeginTransactionAsync();

        // Hủy đơn thì trả hàng về kho.
        if (request.Status == OrderStatus.Cancelled)
        {
            foreach (var item in order.Items)
            {
                var quantity = item.Quantity;
                await db.Products
                    .Where(p => p.Id == item.ProductId)
                    .ExecuteUpdateAsync(set => set.SetProperty(p => p.Stock, p => p.Stock + quantity));
            }
        }

        order.Status = request.Status!.Value;
        order.Address = request.Address.Trim();
        order.DeliveryDate = request.DeliveryDate;
        order.DeliverySlot = request.DeliverySlot;

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return OrderDto.From(order);
    }

    /// <summary>DELETE /api/orders/{id} — chỉ xóa được đơn đã hủy, trả 204 No Content.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null)
            return OrderNotFound(id);

        if (order.Status != OrderStatus.Cancelled)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Không thể xóa đơn",
                detail: "Chỉ xóa được đơn đã hủy. Hãy hủy đơn trước nếu muốn xóa.");
        }

        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private ObjectResult OrderNotFound(int id) => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Không tìm thấy đơn hàng",
        detail: $"Không có đơn hàng nào với mã {id}.");

    private void ValidatePhone(string phone)
    {
        var digits = Regex.Replace(phone, @"[\s.]", "");
        if (!PhonePattern().IsMatch(digits))
            ModelState.AddModelError(nameof(CreateOrderRequest.Phone), "Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0).");
    }

    private void ValidateDelivery(DateOnly date, string slot, bool checkPast)
    {
        if (!ShopRules.DeliverySlots.Contains(slot))
            ModelState.AddModelError(nameof(CreateOrderRequest.DeliverySlot), "Khung giờ giao không hợp lệ.");

        if (checkPast && date < ShopRules.TodayInVietnam)
            ModelState.AddModelError(nameof(CreateOrderRequest.DeliveryDate), "Ngày giao không được ở quá khứ.");
    }
}
