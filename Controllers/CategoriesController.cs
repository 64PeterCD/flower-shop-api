using FlowerShop.Api.Data;
using FlowerShop.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    /// <summary>GET /api/categories — danh sách danh mục.</summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await db.Categories.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
        return categories.Select(CategoryDto.From).ToList();
    }
}
