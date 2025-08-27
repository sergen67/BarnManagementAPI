namespace Barn.Api.Contracts;
public record SellRequest(int ProductId, decimal UnitPrice);
public record SoldItemDto(int Id, string ProductType, decimal Quantity, decimal UnitPrice, decimal Revenue, DateTime SoldAt);