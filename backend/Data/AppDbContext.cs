using Microsoft.EntityFrameworkCore;
using ShoppingApi.Models;

namespace ShoppingApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Id).HasMaxLength(20);
            entity.Property(product => product.Name).HasMaxLength(200).IsRequired();
            entity.Property(product => product.Unit).HasMaxLength(40).IsRequired();
            entity.Property(product => product.UnitPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(stock => stock.ProductId);
            entity.HasOne(stock => stock.Product)
                .WithOne(product => product.Stock)
                .HasForeignKey<Stock>(stock => stock.ProductId);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(item => item.ProductId).IsUnique();
            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(order => order.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
            entity.Property(item => item.LineTotal).HasPrecision(18, 2);
            entity.Property(item => item.ProductId).HasMaxLength(20);
            entity.Property(item => item.ProductName).HasMaxLength(200);
            entity.HasOne(item => item.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.OrderId);
        });
    }
}
