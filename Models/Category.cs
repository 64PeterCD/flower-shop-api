namespace FlowerShop.Api.Models;

public class Category
{
    public int Id { get; set; }

    /// <summary>Tên dùng trên URL của front-end, ví dụ "hoa-bo".</summary>
    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Màu cánh hoa (mã hex) dùng để vẽ hình minh họa ở front-end.</summary>
    public string PetalColor { get; set; } = "#d9506f";

    public List<Product> Products { get; set; } = [];
}
