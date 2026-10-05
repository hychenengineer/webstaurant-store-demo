using Webstaurant.IDS.Api.Domains.Inventory.Models;
using Webstaurant.IDS.Api.Infrastructure.Data;

namespace Webstaurant.IDS.Api.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Products.Any()) return;

        var whLititz = new Warehouse { Code = "PA-LITITZ", Name = "Lititz Distribution Center", City = "Lititz", State = "PA" };
        var whDayton = new Warehouse { Code = "NV-DAYTON", Name = "Dayton Distribution Center", City = "Dayton", State = "NV" };
        var whCumming = new Warehouse { Code = "GA-CUMMING", Name = "Cumming Distribution Center", City = "Cumming", State = "GA" };

        context.Warehouses.AddRange(whLititz, whDayton, whCumming);
        context.SaveChanges();

        var products = new List<Product>
        {
            new Product
            {
                Sku = "177FF40N",
                Name = "Avantco FF40N Natural Gas 40 lb. Stainless Steel Floor Fryer",
                Category = "Cooking Equipment",
                UnitPrice = 949.00m,
                IsDropShipOnly = false,
                ReorderPoint = 10,
                TargetReplenishQty = 25,
                VendorName = "Avantco Equipment"
            },
            new Product
            {
                Sku = "254CAM1826",
                Name = "Cambro Camwear 18\" x 26\" x 6\" Clear Food Storage Box",
                Category = "Storage & Transport",
                UnitPrice = 32.49m,
                IsDropShipOnly = false,
                ReorderPoint = 20,
                TargetReplenishQty = 50,
                VendorName = "Cambro Manufacturing"
            },
            new Product
            {
                Sku = "472COT3072",
                Name = "Regency 30\" x 72\" 16-Gauge 304 Stainless Steel Commercial Work Table",
                Category = "Stainless Steel Tables",
                UnitPrice = 189.00m,
                IsDropShipOnly = false,
                ReorderPoint = 15,
                TargetReplenishQty = 30,
                VendorName = "Regency Tables and Sinks"
            },
            new Product
            {
                Sku = "922V36G",
                Name = "Vulcan 36\" Commercial 6-Burner Gas Range with Standard Oven",
                Category = "Heavy Cooking Equipment",
                UnitPrice = 3850.00m,
                IsDropShipOnly = true,
                ReorderPoint = 0,
                TargetReplenishQty = 0,
                VendorName = "Vulcan Equipment"
            },
            new Product
            {
                Sku = "922VC4GD",
                Name = "Vulcan VC4GD Single Deck Natural Gas Convection Oven",
                Category = "Heavy Cooking Equipment",
                UnitPrice = 6420.00m,
                IsDropShipOnly = true,
                ReorderPoint = 0,
                TargetReplenishQty = 0,
                VendorName = "Vulcan Equipment"
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();

        // Seed initial warehouse balances for stocked items
        var stockBalances = new List<StockBalance>
        {
            // Avantco Fryer in Lititz and Dayton
            new StockBalance { ProductId = products[0].Id, WarehouseId = whLititz.Id, QtyOnHand = 12, QtyReserved = 0, QtyInTransit = 0 },
            new StockBalance { ProductId = products[0].Id, WarehouseId = whDayton.Id, QtyOnHand = 8, QtyReserved = 0, QtyInTransit = 0 },

            // Cambro Food Box in Lititz, Dayton, Cumming
            new StockBalance { ProductId = products[1].Id, WarehouseId = whLititz.Id, QtyOnHand = 45, QtyReserved = 0, QtyInTransit = 0 },
            new StockBalance { ProductId = products[1].Id, WarehouseId = whDayton.Id, QtyOnHand = 25, QtyReserved = 0, QtyInTransit = 0 },
            new StockBalance { ProductId = products[1].Id, WarehouseId = whCumming.Id, QtyOnHand = 30, QtyReserved = 0, QtyInTransit = 0 },

            // Regency Table
            new StockBalance { ProductId = products[2].Id, WarehouseId = whLititz.Id, QtyOnHand = 18, QtyReserved = 0, QtyInTransit = 0 },
            new StockBalance { ProductId = products[2].Id, WarehouseId = whDayton.Id, QtyOnHand = 14, QtyReserved = 0, QtyInTransit = 0 }
        };

        context.StockBalances.AddRange(stockBalances);
        context.SaveChanges();
    }
}
