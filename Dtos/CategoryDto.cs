using FlowerShop.Api.Models;

namespace FlowerShop.Api.Dtos;

public record CategoryDto(int Id, string Slug, string Name, string Description, string PetalColor)
{
    public static CategoryDto From(Category category) =>
        new(category.Id, category.Slug, category.Name, category.Description, category.PetalColor);
}
