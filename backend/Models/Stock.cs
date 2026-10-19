namespace ShoppingApi.Models;

public class Stock
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }

    public Product Product { get; set; } = null!;
}
