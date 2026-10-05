using Microsoft.EntityFrameworkCore;
using Webstaurant.IDS.Api.Domains.Inventory.Models;
using Webstaurant.IDS.Api.Domains.Purchasing.Models;
using Webstaurant.IDS.Api.Domains.Orders.Models;
using Webstaurant.IDS.Api.Domains.EDI.Models;

namespace Webstaurant.IDS.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<VendorPurchaseOrder> PurchaseOrders => Set<VendorPurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<EdiTransaction> EdiTransactions => Set<EdiTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StockBalance>()
            .HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId);

        modelBuilder.Entity<StockBalance>()
            .HasOne(s => s.Warehouse)
            .WithMany()
            .HasForeignKey(s => s.WarehouseId);

        modelBuilder.Entity<VendorPurchaseOrder>()
            .HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(i => i.PurchaseOrderId);

        modelBuilder.Entity<CustomerOrder>()
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.CustomerOrderId);
    }
}
