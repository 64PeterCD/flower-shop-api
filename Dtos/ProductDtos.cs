using System.ComponentModel.DataAnnotations;
using FlowerShop.Api.Models;

namespace FlowerShop.Api.Dtos;

/// <summary>Dữ liệu sản phẩm trả về cho front-end.</summary>
public record ProductDto(
    int Id,
    string Name,
    string Description,
    string Flowers,
    decimal Price,
    int Stock,
    string PetalColor,
    bool Featured,
    int CategoryId,
    string CategorySlug,
    string CategoryName)
{
    public static ProductDto From(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Flowers,
        product.Price,
        product.Stock,
        product.PetalColor,
        product.Featured,
        product.CategoryId,
        product.Category.Slug,
        product.Category.Name);
}

/// <summary>Dữ liệu front-end gửi lên khi thêm (POST) hoặc sửa (PUT) sản phẩm.</summary>
public class ProductRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
    [StringLength(120, ErrorMessage = "Tên sản phẩm tối đa 120 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
    public string Description { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Thành phần tối đa 200 ký tự.")]
    public string Flowers { get; set; } = string.Empty;

    [Range(0, 100_000_000, ErrorMessage = "Giá phải từ 0 đến 100.000.000 đồng.")]
    public decimal Price { get; set; }

    [Range(0, 100_000, ErrorMessage = "Tồn kho phải từ 0 đến 100.000.")]
    public int Stock { get; set; }

    [RegularExpression("^#[0-9a-fA-F]{6}$", ErrorMessage = "Màu hoa phải là mã hex dạng #rrggbb.")]
    public string PetalColor { get; set; } = "#d9506f";

    public bool Featured { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
    public int CategoryId { get; set; }

    public void ApplyTo(Product product)
    {
        product.Name = Name.Trim();
        product.Description = Description.Trim();
        product.Flowers = Flowers.Trim();
        product.Price = Price;
        product.Stock = Stock;
        product.PetalColor = PetalColor;
        product.Featured = Featured;
        product.CategoryId = CategoryId;
    }
}
