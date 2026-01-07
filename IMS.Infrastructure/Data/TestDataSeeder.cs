using IMS.Domain.Entities;
using IMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IMS.Infrastructure.Data
{
    /// <summary>
    /// TIER 3: Test data seeder - ONLY for Development environment
    /// Should be called conditionally in Program.cs
    /// </summary>
    public static class TestDataSeeder
    {
        /// <summary>
        /// Seeds sample data for development/testing purposes
        /// </summary>
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Check if data already exists
            if (await context.Categories.AnyAsync())
                return;

            // Seed Categories
            var categories = new List<Category>
            {
                new Category { Name = "Electronics", Description = "Electronic devices and accessories", IsActive = true },
                new Category { Name = "Furniture", Description = "Office and home furniture", IsActive = true },
                new Category { Name = "Stationery", Description = "Office supplies and stationery", IsActive = true },
                new Category { Name = "Hardware", Description = "Tools and hardware items", IsActive = true },
                new Category { Name = "Software", Description = "Software licenses and subscriptions", IsActive = true }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // Seed Suppliers
            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Name = "Tech Supplies Co.",
                    ContactPerson = "John Smith",
                    Phone = "555-0101",
                    Email = "contact@techsupplies.com",
                    Address = "123 Tech Street, Silicon Valley, CA",
                    IsActive = true
                },
                new Supplier
                {
                    Name = "Office Depot Inc.",
                    ContactPerson = "Sarah Johnson",
                    Phone = "555-0102",
                    Email = "sales@officedepot.com",
                    Address = "456 Commerce Blvd, New York, NY",
                    IsActive = true
                },
                new Supplier
                {
                    Name = "Hardware Hub",
                    ContactPerson = "Mike Wilson",
                    Phone = "555-0103",
                    Email = "info@hardwarehub.com",
                    Address = "789 Industrial Park, Chicago, IL",
                    IsActive = true
                }
            };
            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();

            // Seed Products
            var products = new List<Product>
            {
                // Electronics
                new Product { Name = "Wireless Mouse", SKU = "ELEC-001", Description = "Ergonomic wireless mouse", UnitPrice = 29.99m, CurrentStock = 45, MinimumStock = 10, CategoryId = categories[0].Id, SupplierId = suppliers[0].Id },
                new Product { Name = "USB Keyboard", SKU = "ELEC-002", Description = "Standard USB keyboard", UnitPrice = 39.99m, CurrentStock = 30, MinimumStock = 10, CategoryId = categories[0].Id, SupplierId = suppliers[0].Id },
                new Product { Name = "Monitor 24 inch", SKU = "ELEC-003", Description = "Full HD LED monitor", UnitPrice = 199.99m, CurrentStock = 15, MinimumStock = 5, CategoryId = categories[0].Id, SupplierId = suppliers[0].Id },
                new Product { Name = "USB-C Cable", SKU = "ELEC-004", Description = "High-speed USB-C cable 2m", UnitPrice = 12.99m, CurrentStock = 8, MinimumStock = 15, CategoryId = categories[0].Id, SupplierId = suppliers[0].Id },
                
                // Furniture
                new Product { Name = "Office Chair", SKU = "FURN-001", Description = "Ergonomic office chair with lumbar support", UnitPrice = 249.99m, CurrentStock = 12, MinimumStock = 5, CategoryId = categories[1].Id, SupplierId = suppliers[1].Id },
                new Product { Name = "Standing Desk", SKU = "FURN-002", Description = "Adjustable height standing desk", UnitPrice = 449.99m, CurrentStock = 8, MinimumStock = 3, CategoryId = categories[1].Id, SupplierId = suppliers[1].Id },
                new Product { Name = "Filing Cabinet", SKU = "FURN-003", Description = "4-drawer metal filing cabinet", UnitPrice = 159.99m, CurrentStock = 6, MinimumStock = 5, CategoryId = categories[1].Id, SupplierId = suppliers[1].Id },
                
                // Stationery
                new Product { Name = "A4 Paper Ream", SKU = "STAT-001", Description = "500 sheets premium A4 paper", UnitPrice = 8.99m, CurrentStock = 120, MinimumStock = 30, CategoryId = categories[2].Id, SupplierId = suppliers[1].Id },
                new Product { Name = "Ballpoint Pens (Box)", SKU = "STAT-002", Description = "Box of 50 blue ballpoint pens", UnitPrice = 15.99m, CurrentStock = 25, MinimumStock = 10, CategoryId = categories[2].Id, SupplierId = suppliers[1].Id },
                new Product { Name = "Sticky Notes Pack", SKU = "STAT-003", Description = "Multi-color sticky notes pack", UnitPrice = 6.99m, CurrentStock = 40, MinimumStock = 15, CategoryId = categories[2].Id, SupplierId = suppliers[1].Id },
                new Product { Name = "Stapler Heavy Duty", SKU = "STAT-004", Description = "Heavy duty desktop stapler", UnitPrice = 24.99m, CurrentStock = 7, MinimumStock = 10, CategoryId = categories[2].Id, SupplierId = suppliers[1].Id },
                
                // Hardware
                new Product { Name = "Screwdriver Set", SKU = "HARD-001", Description = "Professional 20-piece screwdriver set", UnitPrice = 34.99m, CurrentStock = 18, MinimumStock = 8, CategoryId = categories[3].Id, SupplierId = suppliers[2].Id },
                new Product { Name = "Power Drill", SKU = "HARD-002", Description = "Cordless power drill with battery", UnitPrice = 89.99m, CurrentStock = 10, MinimumStock = 5, CategoryId = categories[3].Id, SupplierId = suppliers[2].Id },
                new Product { Name = "Measuring Tape", SKU = "HARD-003", Description = "25ft measuring tape", UnitPrice = 12.99m, CurrentStock = 22, MinimumStock = 10, CategoryId = categories[3].Id, SupplierId = suppliers[2].Id },
                
                // Software
                new Product { Name = "Office Suite License", SKU = "SOFT-001", Description = "1-year office productivity suite", UnitPrice = 129.99m, CurrentStock = 50, MinimumStock = 10, CategoryId = categories[4].Id, SupplierId = suppliers[0].Id },
                new Product { Name = "Antivirus Software", SKU = "SOFT-002", Description = "1-year antivirus protection", UnitPrice = 49.99m, CurrentStock = 35, MinimumStock = 15, CategoryId = categories[4].Id, SupplierId = suppliers[0].Id },
                new Product { Name = "Design Software Pro", SKU = "SOFT-003", Description = "Professional design software license", UnitPrice = 299.99m, CurrentStock = 5, MinimumStock = 5, CategoryId = categories[4].Id, SupplierId = suppliers[0].Id },
                
                // Additional products
                new Product { Name = "Laptop Bag", SKU = "ELEC-005", Description = "Padded laptop bag 15 inch", UnitPrice = 45.99m, CurrentStock = 20, MinimumStock = 8, CategoryId = categories[0].Id, SupplierId = suppliers[0].Id },
                new Product { Name = "Whiteboard", SKU = "FURN-004", Description = "Magnetic whiteboard 4x6 ft", UnitPrice = 89.99m, CurrentStock = 4, MinimumStock = 3, CategoryId = categories[1].Id, SupplierId = suppliers[1].Id },
                new Product { Name = "Calculator", SKU = "STAT-005", Description = "Scientific calculator", UnitPrice = 18.99m, CurrentStock = 15, MinimumStock = 10, CategoryId = categories[2].Id, SupplierId = suppliers[1].Id }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // Seed Sample Stock Transactions
            var transactions = new List<StockTransaction>
            {
                new StockTransaction { ProductId = products[0].Id, TransactionType = TransactionType.StockIn, Quantity = 50, Remarks = "Initial stock", StockBefore = 0, StockAfter = 50, CreatedBy = "System", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                new StockTransaction { ProductId = products[0].Id, TransactionType = TransactionType.StockOut, Quantity = 5, Remarks = "Sales order #1001", StockBefore = 50, StockAfter = 45, CreatedBy = "System", CreatedDate = DateTime.UtcNow.AddDays(-5) },
                new StockTransaction { ProductId = products[3].Id, TransactionType = TransactionType.StockIn, Quantity = 20, Remarks = "Initial stock", StockBefore = 0, StockAfter = 20, CreatedBy = "System", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                new StockTransaction { ProductId = products[3].Id, TransactionType = TransactionType.StockOut, Quantity = 12, Remarks = "Bulk order", StockBefore = 20, StockAfter = 8, CreatedBy = "System", CreatedDate = DateTime.UtcNow.AddDays(-3) },
                new StockTransaction { ProductId = products[7].Id, TransactionType = TransactionType.StockIn, Quantity = 150, Remarks = "Supplier delivery", StockBefore = 0, StockAfter = 150, CreatedBy = "System", CreatedDate = DateTime.UtcNow.AddDays(-15) },
                new StockTransaction { ProductId = products[7].Id, TransactionType = TransactionType.StockOut, Quantity = 30, Remarks = "Office consumption", StockBefore = 150, StockAfter = 120, CreatedBy = "System", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                new StockTransaction { ProductId = products[10].Id, TransactionType = TransactionType.StockIn, Quantity = 15, Remarks = "Restock", StockBefore = 0, StockAfter = 15, CreatedBy = "System", CreatedDate = DateTime.UtcNow.AddDays(-12) },
                new StockTransaction { ProductId = products[10].Id, TransactionType = TransactionType.StockOut, Quantity = 8, Remarks = "Department order", StockBefore = 15, StockAfter = 7, CreatedBy = "System", CreatedDate = DateTime.UtcNow.AddDays(-4) },
                new StockTransaction { ProductId = products[17].Id, TransactionType = TransactionType.Adjustment, Quantity = 2, Remarks = "Inventory audit adjustment", StockBefore = 2, StockAfter = 4, CreatedBy = "System", CreatedDate = DateTime.UtcNow.AddDays(-2) },
                new StockTransaction { ProductId = products[15].Id, TransactionType = TransactionType.StockOut, Quantity = 45, Remarks = "License distribution", StockBefore = 50, StockAfter = 5, CreatedBy = "System", CreatedDate = DateTime.UtcNow.AddDays(-6) }
            };
            await context.StockTransactions.AddRangeAsync(transactions);
            await context.SaveChangesAsync();
        }
    }
}
