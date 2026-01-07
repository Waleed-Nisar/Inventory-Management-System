// ============================================================================
// PART 2: DATA LAYER (Infrastructure) - SMART SEEDING
// ============================================================================

// FILE: IMS.Infrastructure/Data/ApplicationDbContext.cs
// ============================================================================
using IMS.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IMS.Infrastructure.Data
{
    /// <summary>
    /// Application database context with Identity integration
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Category configuration
            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired();
            });

            // Supplier configuration
            builder.Entity<Supplier>(entity =>
            {
                entity.HasIndex(e => e.Name);
                entity.Property(e => e.Name).IsRequired();
            });

            // Product configuration
            builder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.HasIndex(e => e.Name);
                
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // StockTransaction configuration
            builder.Entity<StockTransaction>(entity =>
            {
                entity.HasIndex(e => e.CreatedDate);
                entity.HasIndex(e => e.ProductId);
                
                entity.HasOne(st => st.Product)
                    .WithMany(p => p.StockTransactions)
                    .HasForeignKey(st => st.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

// FILE: IMS.Infrastructure/Repositories/IRepository.cs
// ============================================================================
using System.Linq.Expressions;

namespace IMS.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository interface for common CRUD operations
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
    }
}

// FILE: IMS.Infrastructure/Repositories/Repository.cs
// ============================================================================
using IMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IMS.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity != null;
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }
    }
}

// FILE: IMS.Infrastructure/Repositories/IProductRepository.cs
// ============================================================================
using IMS.Domain.Entities;

namespace IMS.Infrastructure.Repositories
{
    /// <summary>
    /// Product-specific repository interface
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsWithDetailsAsync();
        Task<Product?> GetProductWithDetailsAsync(int id);
        Task<IEnumerable<Product>> GetLowStockProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<bool> SKUExistsAsync(string sku, int? excludeId = null);
    }
}

// FILE: IMS.Infrastructure/Repositories/ProductRepository.cs
// ============================================================================
using IMS.Domain.Entities;
using IMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS.Infrastructure.Repositories
{
    /// <summary>
    /// Product repository with specific queries
    /// </summary>
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsWithDetailsAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.StockTransactions.OrderByDescending(st => st.CreatedDate).Take(10))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CurrentStock < p.MinimumStock)
                .OrderBy(p => p.CurrentStock)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.Supplier)
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> SKUExistsAsync(string sku, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return false;

            return await _dbSet.AnyAsync(p => 
                p.SKU == sku && 
                (!excludeId.HasValue || p.Id != excludeId.Value));
        }
    }
}

// FILE: IMS.Infrastructure/Data/DbSeeder.cs
// ============================================================================
using IMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace IMS.Infrastructure.Data
{
    /// <summary>
    /// TIER 1: Critical system data seeder (ALWAYS runs)
    /// Seeds essential data required for the application to function
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>
        /// Seeds essential system data (Roles) - runs in ALL environments
        /// </summary>
        public static async Task SeedEssentialDataAsync(RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles (Required for authorization)
            foreach (var roleName in UserRole.GetAllRoles())
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

#if DEBUG
        /// <summary>
        /// TIER 2: Seeds debug admin user - ONLY in DEBUG builds
        /// This method is completely excluded from Release builds
        /// </summary>
        public static async Task SeedDebugAdminAsync(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            const string adminEmail = "admin@ims.com";
            const string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRole.Admin);
                }
            }
        }
#endif
    }
}

// FILE: IMS.Infrastructure/Data/TestDataSeeder.cs
// ============================================================================
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

// FILE: IMS.Web/appsettings.json
// ============================================================================
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InventoryManagementDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApplicationSettings": {
    "ApplicationName": "Inventory Management System",
    "Version": "1.0.0"
  }
}

// FILE: IMS.Web/appsettings.Development.json
// ============================================================================
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InventoryManagementDB_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "SeedTestData": true
}

// FILE: IMS.Infrastructure/Data/SeedScripts/001_SeedRoles.sql
// ============================================================================
-- ============================================================
-- 001_SeedRoles.sql
-- TIER 1: Essential system data (Required for authorization)
-- Run this script in ALL environments (Dev, Staging, Prod)
-- ============================================================

-- Seed AspNetRoles (Required for application to function)
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID())
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Manager')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Manager', 'MANAGER', NEWID())
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Staff')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Staff', 'STAFF', NEWID())
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Viewer')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Viewer', 'VIEWER', NEWID())
END

PRINT 'Essential roles seeded successfully'
GO

// FILE: IMS.Infrastructure/Data/SeedScripts/002_SeedTestData.sql
// ============================================================================
-- ============================================================
-- 002_SeedTestData.sql
-- TIER 3: Sample/Test data (Development/Testing ONLY)
-- DO NOT run this script in Production
-- ============================================================

-- Seed Categories
SET IDENTITY_INSERT Categories ON

INSERT INTO Categories (Id, Name, Description, IsActive, CreatedDate) VALUES
(1, 'Electronics', 'Electronic devices and accessories', 1, GETUTCDATE()),
(2, 'Furniture', 'Office and home furniture', 1, GETUTCDATE()),
(3, 'Stationery', 'Office supplies and stationery', 1, GETUTCDATE()),
(4, 'Hardware', 'Tools and hardware items', 1, GETUTCDATE()),
(5, 'Software', 'Software licenses and subscriptions', 1, GETUTCDATE())

SET IDENTITY_INSERT Categories OFF

-- Seed Suppliers
SET IDENTITY_INSERT Suppliers ON

INSERT INTO Suppliers (Id, Name, ContactPerson, Phone, Email, Address, IsActive, CreatedDate) VALUES
(1, 'Tech Supplies Co.', 'John Smith', '555-0101', 'contact@techsupplies.com', '123 Tech Street, Silicon Valley, CA', 1, GETUTCDATE()),
(2, 'Office Depot Inc.', 'Sarah Johnson', '555-0102', 'sales@officedepot.com', '456 Commerce Blvd, New York, NY', 1, GETUTCDATE()),
(3, 'Hardware Hub', 'Mike Wilson', '555-0103', 'info@hardwarehub.com', '789 Industrial Park, Chicago, IL', 1, GETUTCDATE())

SET IDENTITY_INSERT Suppliers OFF

-- Seed Products (Sample selection)
SET IDENTITY_INSERT Products ON

INSERT INTO Products (Id, Name, SKU, Description, UnitPrice, CurrentStock, MinimumStock, CategoryId, SupplierId, IsActive, CreatedDate) VALUES
(1, 'Wireless Mouse', 'ELEC-001', 'Ergonomic wireless mouse', 29.99, 45, 10, 1, 1, 1, GETUTCDATE()),
(2, 'USB Keyboard', 'ELEC-002', 'Standard USB keyboard', 39.99, 30, 10, 1, 1, 1, GETUTCDATE()),
(3, 'Office Chair', 'FURN-001', 'Ergonomic office chair with lumbar support', 249.99, 12, 5, 2, 2, 1, GETUTCDATE()),
(4, 'A4 Paper Ream', 'STAT-001', '500 sheets premium A4 paper', 8.99, 120, 30, 3, 2, 1, GETUTCDATE()),
(5, 'USB-C Cable', 'ELEC-004', 'High-speed USB-C cable 2m', 12.99, 8, 15, 1, 1, 1, GETUTCDATE())

SET IDENTITY_INSERT Products OFF

PRINT 'Test data seeded successfully'
PRINT 'WARNING: This script should ONLY be used in Development/Testing environments'
GO
