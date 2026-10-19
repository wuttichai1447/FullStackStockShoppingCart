using Microsoft.EntityFrameworkCore;
using ShoppingApi.Models;

namespace ShoppingApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Products.AnyAsync())
        {
            return;
        }

        var products = new (string Id, string Name, decimal Price, string Unit, int Quantity)[]
        {
            ("P001", "น้ำดื่ม 600 มล.", 10m, "ขวด", 50),
            ("P002", "กาแฟกระป๋อง", 25m, "กระป๋อง", 30),
            ("P003", "ขนมปังแผ่น", 35m, "ถุง", 12),
            ("P004", "นมสด 200 มล.", 18m, "กล่อง", 40),
            ("P005", "ข้าวสาร 5 กก.", 189m, "ถุง", 8),
            ("P006", "น้ำมันพืช 1 ลิตร", 62m, "ขวด", 15),
            ("P007", "บะหมี่กึ่งสำเร็จรูป", 7m, "ซอง", 100),
            ("P008", "ไข่ไก่", 5m, "ฟอง", 36)
        };

        foreach (var item in products)
        {
            db.Products.Add(new Product
            {
                Id = item.Id,
                Name = item.Name,
                UnitPrice = item.Price,
                Unit = item.Unit,
                Stock = new Stock
                {
                    ProductId = item.Id,
                    Quantity = item.Quantity,
                    ReservedQuantity = 0
                }
            });
        }

        await db.SaveChangesAsync();
    }
}
