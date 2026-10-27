namespace FlowerShop.Api.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Thành phần hoa, ví dụ "Hồng đỏ, baby trắng, lá bạc".</summary>
    public string Flowers { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string PetalColor { get; set; } = "#d9506f";

    /// <summary>Hiện ở mục "Sản phẩm nổi bật" trên trang chủ.</summary>
    public bool Featured { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;
}
