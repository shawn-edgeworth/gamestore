using GameStore.Api.Models;

namespace GameStore.Api.Mappers;

public static class CategoryMapper
{
    public static CategoryDetailsResponse ToDetailsResponse(this Category category)
    {
        return new CategoryDetailsResponse
        {
            Id = category.Id,
            Name = category.Name,
        };
    }

    public static CategoryCollectionResponse ToCollectionResponse(this IList<Category> categories)
    {
        return new CategoryCollectionResponse()
        {
            Categories = categories.Select(x => x.ToDetailsResponse()).ToList()
        };
    }
}