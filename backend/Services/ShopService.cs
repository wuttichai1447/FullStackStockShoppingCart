using Microsoft.EntityFrameworkCore;
using ShoppingApi.Data;
using ShoppingApi.Dtos;
using ShoppingApi.Models;

namespace ShoppingApi.Services;

public class ShopService
{
    private readonly AppDbContext _db;

    public ShopService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync()
    {
        var products = await _db.Products
            .AsNoTracking()
            .Include(product => product.Stock)
            .OrderBy(product => product.Id)
            .ToListAsync();

        return products.Select(ToProductDto).ToList();
    }

    public async Task<CartDto> GetCartAsync()
    {
        var items = await LoadCartAsync(tracking: false);
        return ToCartDto(items);
    }

    public async Task<CartDto> AddToCartAsync(string productId, int quantity)
    {
        if (quantity < 1)
        {
            throw new ShopException("จำนวนสินค้าต้องมากกว่า 0");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var stock = await GetStockForUpdateAsync(productId);
        var available = Available(stock);
        if (available < quantity)
        {
            throw new ShopException(
                available == 0
                    ? "สินค้าหมด ไม่สามารถเพิ่มลงตะกร้าได้"
                    : $"สินค้าคงเหลือไม่เพียงพอ เพิ่มได้อีก {available} {stock.Product.Unit}");
        }

        stock.ReservedQuantity += quantity;

        var cartItem = await _db.CartItems.FirstOrDefaultAsync(item => item.ProductId == productId);
        if (cartItem is null)
        {
            _db.CartItems.Add(new CartItem
            {
                ProductId = productId,
                Quantity = quantity
            });
        }
        else
        {
            cartItem.Quantity += quantity;
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetCartAsync();
    }

    public async Task<CartDto> SetQuantityAsync(string productId, int quantity)
    {
        if (quantity < 0)
        {
            throw new ShopException("จำนวนสินค้าต้องไม่ติดลบ");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var stock = await GetStockForUpdateAsync(productId);
        var cartItem = await _db.CartItems.FirstOrDefaultAsync(item => item.ProductId == productId);
        var current = cartItem?.Quantity ?? 0;
        var delta = quantity - current;

        if (delta > 0 && Available(stock) < delta)
        {
            var available = Available(stock);
            throw new ShopException(
                available == 0
                    ? "สินค้าหมด ไม่สามารถเพิ่มจำนวนในตะกร้าได้"
                    : $"สินค้าคงเหลือไม่เพียงพอ เพิ่มได้อีก {available} {stock.Product.Unit}");
        }

        stock.ReservedQuantity += delta;

        if (quantity == 0)
        {
            if (cartItem is not null)
            {
                _db.CartItems.Remove(cartItem);
            }
        }
        else if (cartItem is null)
        {
            _db.CartItems.Add(new CartItem
            {
                ProductId = productId,
                Quantity = quantity
            });
        }
        else
        {
            cartItem.Quantity = quantity;
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetCartAsync();
    }

    public async Task<CartDto> RemoveItemAsync(string productId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var cartItem = await _db.CartItems.FirstOrDefaultAsync(item => item.ProductId == productId);
        if (cartItem is null)
        {
            throw new ShopException("ไม่พบสินค้านี้ในตะกร้า", StatusCodes.Status404NotFound);
        }

        var stock = await GetStockForUpdateAsync(productId);
        stock.ReservedQuantity -= cartItem.Quantity;
        if (stock.ReservedQuantity < 0)
        {
            stock.ReservedQuantity = 0;
        }

        _db.CartItems.Remove(cartItem);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetCartAsync();
    }

    public async Task<CartDto> ClearCartAsync()
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var items = await _db.CartItems.ToListAsync();
        if (items.Count == 0)
        {
            return ToCartDto([]);
        }

        var productIds = items.Select(item => item.ProductId).ToList();
        var stocks = await _db.Stocks
            .Where(stock => productIds.Contains(stock.ProductId))
            .ToListAsync();

        foreach (var item in items)
        {
            var stock = stocks.First(stock => stock.ProductId == item.ProductId);
            stock.ReservedQuantity -= item.Quantity;
            if (stock.ReservedQuantity < 0)
            {
                stock.ReservedQuantity = 0;
            }
        }

        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetCartAsync();
    }

    public async Task<CheckoutDto> CheckoutAsync()
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var items = await _db.CartItems
            .Include(item => item.Product)
            .ThenInclude(product => product.Stock)
            .ToListAsync();

        if (items.Count == 0)
        {
            throw new ShopException("ตะกร้าว่าง ไม่มียอดให้ชำระ");
        }

        var order = new Order
        {
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in items)
        {
            var stock = item.Product.Stock
                ?? throw new ShopException($"ไม่พบสต็อกของสินค้า {item.Product.Name}");

            if (stock.Quantity < item.Quantity)
            {
                throw new ShopException($"สต็อกของ {item.Product.Name} ไม่พอสำหรับตัดยอด");
            }

            stock.Quantity -= item.Quantity;
            stock.ReservedQuantity -= item.Quantity;
            if (stock.ReservedQuantity < 0)
            {
                stock.ReservedQuantity = 0;
            }

            var lineTotal = item.Product.UnitPrice * item.Quantity;
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                UnitPrice = item.Product.UnitPrice,
                Quantity = item.Quantity,
                LineTotal = lineTotal
            });
        }

        order.TotalAmount = order.Items.Sum(item => item.LineTotal);
        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        var units = items.ToDictionary(item => item.ProductId, item => item.Product.Unit);
        return new CheckoutDto(
            order.Id,
            order.TotalAmount,
            order.CreatedAt,
            order.Items.Select(item => new CheckoutItemDto(
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                units[item.ProductId],
                item.Quantity,
                item.LineTotal)).ToList());
    }

    private async Task<Stock> GetStockForUpdateAsync(string productId)
    {
        var stock = await _db.Stocks
            .Include(item => item.Product)
            .FirstOrDefaultAsync(item => item.ProductId == productId);

        if (stock is null)
        {
            throw new ShopException("ไม่พบสินค้า", StatusCodes.Status404NotFound);
        }

        return stock;
    }

    private async Task<List<CartItem>> LoadCartAsync(bool tracking)
    {
        var query = _db.CartItems
            .Include(item => item.Product)
            .ThenInclude(product => product.Stock)
            .OrderBy(item => item.ProductId)
            .AsQueryable();

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    private static int Available(Stock stock) => stock.Quantity - stock.ReservedQuantity;

    private static ProductDto ToProductDto(Product product)
    {
        var stock = product.Stock;
        var quantity = stock?.Quantity ?? 0;
        var reserved = stock?.ReservedQuantity ?? 0;
        return new ProductDto(
            product.Id,
            product.Name,
            product.UnitPrice,
            product.Unit,
            quantity,
            reserved,
            quantity - reserved);
    }

    private static CartDto ToCartDto(IReadOnlyList<CartItem> items)
    {
        var lines = items.Select(item =>
        {
            var stock = item.Product.Stock;
            var available = (stock?.Quantity ?? 0) - (stock?.ReservedQuantity ?? 0);
            return new CartItemDto(
                item.ProductId,
                item.Product.Name,
                item.Product.UnitPrice,
                item.Product.Unit,
                item.Quantity,
                item.Product.UnitPrice * item.Quantity,
                available);
        }).ToList();

        return new CartDto(lines, lines.Sum(line => line.LineTotal), lines.Sum(line => line.Quantity));
    }
}
