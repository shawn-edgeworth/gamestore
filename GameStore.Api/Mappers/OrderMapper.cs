using GameStore.Api.Models;

namespace GameStore.Api.Mappers;

public static class OrderMapper
{
    public static CreateOrderResponse ToResponse(this Order order)
    {
        return new CreateOrderResponse
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(x => x.ToDetails()).ToList()
        };
    }

    private static OrderItemDetails ToDetails(this OrderItem orderItem)
    {
        return new OrderItemDetails
        {
            GameId = orderItem.GameId,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice
        };
    }
}