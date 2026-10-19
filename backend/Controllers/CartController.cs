using Microsoft.AspNetCore.Mvc;
using ShoppingApi.Dtos;
using ShoppingApi.Services;

namespace ShoppingApi.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ShopService _shop;

    public CartController(ShopService shop)
    {
        _shop = shop;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        return Ok(await _shop.GetCartAsync());
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] QuantityRequest request)
    {
        var cart = await _shop.AddToCartAsync(request.ProductId, request.Quantity);
        return Ok(cart);
    }

    [HttpPut("items/{productId}")]
    public async Task<IActionResult> SetQuantity(string productId, [FromBody] SetQuantityRequest request)
    {
        var cart = await _shop.SetQuantityAsync(productId, request.Quantity);
        return Ok(cart);
    }

    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItem(string productId)
    {
        var cart = await _shop.RemoveItemAsync(productId);
        return Ok(cart);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var cart = await _shop.ClearCartAsync();
        return Ok(cart);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var receipt = await _shop.CheckoutAsync();
        return Ok(receipt);
    }
}
