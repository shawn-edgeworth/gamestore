using GameStore.Api.Models;

namespace GameStore.Api.Mappers;

public static class OrderMapper
{

    public static OrderCollectionResponse ToCollectionResponse(this IList<Order> orders)
    {
        return new OrderCollectionResponse
        {
            Orders = orders.Select(x=>x.ToDetails()).ToList()
        };
    }

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
            GameName = orderItem.Game.Name,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice
        };
    }

    private static OrderDetails ToDetails(this Order order)
    {
        return new OrderDetails
        {
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(x=>x.ToDetails()).ToList()
        };
    }
}