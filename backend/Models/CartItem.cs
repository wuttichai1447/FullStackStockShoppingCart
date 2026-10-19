namespace ShoppingApi.Models;

public class CartItem
{
    public int Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }

    public Product Product { get; set; } = null!;
}
