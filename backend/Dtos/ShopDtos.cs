namespace ShoppingApi.Dtos;

public record ProductDto(
    string Id,
    string Name,
    decimal UnitPrice,
    string Unit,
    int StockQuantity,
    int ReservedQuantity,
    int AvailableQuantity);

public record CartItemDto(
    string ProductId,
    string Name,
    decimal UnitPrice,
    string Unit,
    int Quantity,
    decimal LineTotal,
    int AvailableQuantity);

public record CartDto(
    IReadOnlyList<CartItemDto> Items,
    decimal TotalAmount,
    int ItemCount);

public record CheckoutItemDto(
    string ProductId,
    string ProductName,
    decimal UnitPrice,
    string Unit,
    int Quantity,
    decimal LineTotal);

public record CheckoutDto(
    int OrderId,
    decimal TotalAmount,
    DateTime CheckedOutAt,
    IReadOnlyList<CheckoutItemDto> Items);

public record QuantityRequest(string ProductId, int Quantity);

public record SetQuantityRequest(int Quantity);
