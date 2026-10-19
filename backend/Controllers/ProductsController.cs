using Microsoft.AspNetCore.Mvc;
using ShoppingApi.Services;

namespace ShoppingApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ShopService _shop;

    public ProductsController(ShopService shop)
    {
        _shop = shop;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _shop.GetProductsAsync();
        return Ok(products);
    }
}
