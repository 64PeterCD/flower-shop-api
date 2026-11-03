using FlowerShop.Api.Data;
using FlowerShop.Api.Dtos;
using FlowerShop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    /// <summary>GET /api/products?category=hoa-bo&amp;search=hong&amp;featured=true — danh sách, có lọc.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll([FromQuery] string? category, [FromQuery] string? search, [FromQuery] bool? featured)
    {
        var query = db.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category.Slug == category);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(keyword));
        }

        if (featured is not null)
            query = query.Where(p => p.Featured == featured);

        var products = await query.OrderBy(p => p.Id).ToListAsync();
        return products.Select(ProductDto.From).ToList();
    }

    /// <summary>GET /api/products/{id} — chi tiết một sản phẩm.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await db.Products.AsNoTracking().Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        return product is null ? ProductNotFound(id) : ProductDto.From(product);
    }

    /// <summary>POST /api/products — thêm sản phẩm mới, trả 201 Created. Chỉ Admin.</summary>
    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductRequest request)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            return CategoryInvalid();

        var product = new Product();
        request.ApplyTo(product);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        await db.Entry(product).Reference(p => p.Category).LoadAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, ProductDto.From(product));
    }

    /// <summary>PUT /api/products/{id} — thay toàn bộ thông tin sản phẩm. Chỉ Admin.</summary>
    [Authorize(Roles = UserRoles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductRequest request)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
            return ProductNotFound(id);

        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            return CategoryInvalid();

        request.ApplyTo(product);
        await db.SaveChangesAsync();

        await db.Entry(product).Reference(p => p.Category).LoadAsync();
        return ProductDto.From(product);
    }

    /// <summary>DELETE /api/products/{id} — xóa sản phẩm chưa từng được đặt, trả 204 No Content. Chỉ Admin.</summary>
    [Authorize(Roles = UserRoles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
            return ProductNotFound(id);

        if (await db.OrderItems.AnyAsync(i => i.ProductId == id))
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Không thể xóa sản phẩm",
                detail: "Sản phẩm đã nằm trong đơn hàng nên không xóa được. Hãy đặt tồn kho về 0 nếu muốn ngừng bán.");
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private ObjectResult ProductNotFound(int id) => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Không tìm thấy sản phẩm",
        detail: $"Không có sản phẩm nào với mã {id}.");

    private ActionResult CategoryInvalid()
    {
        ModelState.AddModelError(nameof(ProductRequest.CategoryId), "Danh mục không tồn tại.");
        return ValidationProblem(ModelState);
    }
}
