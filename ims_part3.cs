// ============================================================================
// PART 3: BUSINESS LAYER (Application)
// ============================================================================

// FILE: IMS.Application/Interfaces/IProductService.cs
// ============================================================================


// FILE: IMS.Application/Interfaces/ICategoryService.cs
// ============================================================================
using IMS.Domain.Entities;

namespace IMS.Application.Interfaces
{
    /// <summary>
    /// Category service interface
    /// </summary>
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> CategoryExistsAsync(int id);
        Task<bool> CategoryHasProductsAsync(int id);
    }
}

// FILE: IMS.Application/Interfaces/IStockService.cs
// ============================================================================
using IMS.Application.ViewModels;
using IMS.Domain.Entities;

namespace IMS.Application.Interfaces
{
    /// <summary>
    /// Stock transaction service interface
    /// </summary>
    public interface IStockService
    {
        Task<StockTransactionViewModel> CreateTransactionAsync(StockTransactionViewModel model, string userName);
        Task<IEnumerable<StockTransaction>> GetTransactionsByProductAsync(int productId);
        Task<IEnumerable<StockTransaction>> GetRecentTransactionsAsync(int count = 10);
        Task<DashboardViewModel> GetDashboardDataAsync();
    }
}

// FILE: IMS.Application/Services/ProductService.cs
// ============================================================================
using IMS.Application.Interfaces;
using IMS.Application.ViewModels;
using IMS.Domain.Entities;
using IMS.Infrastructure.Repositories;

namespace IMS.Application.Services
{
    /// <summary>
    /// Product service implementation
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Supplier> _supplierRepository;

        public ProductService(
            IProductRepository productRepository,
            IRepository<Category> categoryRepository,
            IRepository<Supplier> supplierRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _supplierRepository = supplierRepository;
        }

        public async Task<IEnumerable<ProductViewModel>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetProductsWithDetailsAsync();
            return products.Select(MapToViewModel);
        }

        public async Task<ProductViewModel?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetProductWithDetailsAsync(id);
            return product != null ? MapToViewModel(product) : null;
        }

        public async Task<ProductViewModel> CreateProductAsync(ProductViewModel model)
        {
            try
            {
                var product = new Product
                {
                    Name = model.Name,
                    SKU = model.SKU,
                    Description = model.Description,
                    UnitPrice = model.UnitPrice,
                    CurrentStock = model.CurrentStock,
                    MinimumStock = model.MinimumStock,
                    CategoryId = model.CategoryId,
                    SupplierId = model.SupplierId,
                    IsActive = model.IsActive,
                    CreatedDate = DateTime.UtcNow
                };

                var createdProduct = await _productRepository.AddAsync(product);
                
                // Reload with navigation properties
                var productWithDetails = await _productRepository.GetProductWithDetailsAsync(createdProduct.Id);
                return MapToViewModel(productWithDetails!);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating product: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateProductAsync(ProductViewModel model)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(model.Id);
                if (product == null)
                    return false;

                product.Name = model.Name;
                product.SKU = model.SKU;
                product.Description = model.Description;
                product.UnitPrice = model.UnitPrice;
                product.MinimumStock = model.MinimumStock;
                product.CategoryId = model.CategoryId;
                product.SupplierId = model.SupplierId;
                product.IsActive = model.IsActive;
                product.ModifiedDate = DateTime.UtcNow;

                await _productRepository.UpdateAsync(product);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating product: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return false;

                // Business rule: Cannot delete products with stock
                if (product.CurrentStock > 0)
                    throw new InvalidOperationException("Cannot delete product with existing stock. Reduce stock to zero first.");

                await _productRepository.DeleteAsync(product);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting product: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ProductViewModel>> GetLowStockProductsAsync()
        {
            var products = await _productRepository.GetLowStockProductsAsync();
            return products.Select(MapToViewModel);
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _productRepository.ExistsAsync(id);
        }

        public async Task<bool> SKUExistsAsync(string sku, int? excludeId = null)
        {
            return await _productRepository.SKUExistsAsync(sku, excludeId);
        }

        private static ProductViewModel MapToViewModel(Product product)
        {
            return new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                UnitPrice = product.UnitPrice,
                CurrentStock = product.CurrentStock,
                MinimumStock = product.MinimumStock,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "Unknown",
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name,
                IsActive = product.IsActive,
                IsLowStock = product.IsLowStock,
                CreatedDate = product.CreatedDate,
                ModifiedDate = product.ModifiedDate
            };
        }
    }
}

// FILE: IMS.Application/Services/CategoryService.cs
// ============================================================================
using IMS.Application.Interfaces;
using IMS.Domain.Entities;
using IMS.Infrastructure.Data;
using IMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.Services
{
    /// <summary>
    /// Category service implementation
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly ApplicationDbContext _context;

        public CategoryService(IRepository<Category> categoryRepository, ApplicationDbContext context)
        {
            _categoryRepository = categoryRepository;
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _categoryRepository.FindAsync(c => c.IsActive);
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            try
            {
                category.CreatedDate = DateTime.UtcNow;
                return await _categoryRepository.AddAsync(category);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating category: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                var existing = await _categoryRepository.GetByIdAsync(category.Id);
                if (existing == null)
                    return false;

                existing.Name = category.Name;
                existing.Description = category.Description;
                existing.IsActive = category.IsActive;
                existing.ModifiedDate = DateTime.UtcNow;

                await _categoryRepository.UpdateAsync(existing);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating category: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                    return false;

                // Business rule: Cannot delete category with products
                if (await CategoryHasProductsAsync(id))
                    throw new InvalidOperationException("Cannot delete category with existing products.");

                await _categoryRepository.DeleteAsync(category);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting category: {ex.Message}", ex);
            }
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _categoryRepository.ExistsAsync(id);
        }

        public async Task<bool> CategoryHasProductsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == id);
        }
    }
}

// FILE: IMS.Application/Services/StockService.cs
// ============================================================================
using IMS.Application.Interfaces;
using IMS.Application.ViewModels;
using IMS.Domain.Entities;
using IMS.Domain.Enums;
using IMS.Infrastructure.Data;
using IMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.Services
{
    /// <summary>
    /// Stock transaction service implementation
    /// </summary>
    public class StockService : IStockService
    {
        private readonly IRepository<StockTransaction> _transactionRepository;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public StockService(
            IRepository<StockTransaction> transactionRepository,
            IProductRepository productRepository,
            ApplicationDbContext context)
        {
            _transactionRepository = transactionRepository;
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<StockTransactionViewModel> CreateTransactionAsync(StockTransactionViewModel model, string userName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var product = await _productRepository.GetByIdAsync(model.ProductId);
                if (product == null)
                    throw new InvalidOperationException("Product not found.");

                int stockBefore = product.CurrentStock;
                int stockAfter = stockBefore;

                // Calculate new stock based on transaction type
                switch (model.TransactionType)
                {
                    case TransactionType.StockIn:
                        stockAfter = stockBefore + model.Quantity;
                        break;
                    
                    case TransactionType.StockOut:
                        stockAfter = stockBefore - model.Quantity;
                        if (stockAfter < 0)
                            throw new InvalidOperationException("Insufficient stock. Cannot reduce stock below zero.");
                        break;
                    
                    case TransactionType.Adjustment:
                        stockAfter = model.Quantity; // For adjustments, Quantity represents the new total
                        model.Quantity = Math.Abs(stockAfter - stockBefore);
                        break;
                }

                // Create transaction record
                var stockTransaction = new StockTransaction
                {
                    ProductId = model.ProductId,
                    TransactionType = model.TransactionType,
                    Quantity = model.Quantity,
                    Remarks = model.Remarks ?? string.Empty,
                    StockBefore = stockBefore,
                    StockAfter = stockAfter,
                    CreatedBy = userName,
                    CreatedDate = DateTime.UtcNow
                };

                await _transactionRepository.AddAsync(stockTransaction);

                // Update product stock
                product.CurrentStock = stockAfter;
                product.ModifiedDate = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product);

                await transaction.CommitAsync();

                model.Id = stockTransaction.Id;
                model.StockBefore = stockBefore;
                model.StockAfter = stockAfter;
                model.CreatedDate = stockTransaction.CreatedDate;

                return model;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Error creating stock transaction: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<StockTransaction>> GetTransactionsByProductAsync(int productId)
        {
            return await _context.StockTransactions
                .Include(st => st.Product)
                .Where(st => st.ProductId == productId)
                .OrderByDescending(st => st.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockTransaction>> GetRecentTransactionsAsync(int count = 10)
        {
            return await _context.StockTransactions
                .Include(st => st.Product)
                .ThenInclude(p => p.Category)
                .OrderByDescending(st => st.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
            var totalCategories = await _context.Categories.CountAsync(c => c.IsActive);
            var lowStockCount = await _context.Products.CountAsync(p => p.IsActive && p.CurrentStock < p.MinimumStock);
            var totalStockValue = await _context.Products
                .Where(p => p.IsActive)
                .SumAsync(p => p.CurrentStock * p.UnitPrice);

            var lowStockProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CurrentStock < p.MinimumStock)
                .OrderBy(p => p.CurrentStock)
                .Take(5)
                .ToListAsync();

            var recentTransactions = await GetRecentTransactionsAsync(10);

            return new DashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                LowStockProductsCount = lowStockCount,
                TotalStockValue = totalStockValue,
                LowStockProducts = lowStockProducts,
                RecentTransactions = recentTransactions.ToList()
            };
        }
    }
}

// FILE: IMS.Application/ViewModels/ProductViewModel.cs
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace IMS.Application.ViewModels
{
    /// <summary>
    /// Product view model for CRUD operations
    /// </summary>
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        [Display(Name = "SKU Code")]
        public string? SKU { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999,999.99")]
        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Current stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be 0 or greater")]
        [Display(Name = "Current Stock")]
        public int CurrentStock { get; set; }

        [Required(ErrorMessage = "Minimum stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock must be 0 or greater")]
        [Display(Name = "Minimum Stock Level")]
        public int MinimumStock { get; set; } = 10;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        [Display(Name = "Supplier")]
        public string? SupplierName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public bool IsLowStock { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Last Modified")]
        public DateTime? ModifiedDate { get; set; }
    }
}

// FILE: IMS.Application/ViewModels/StockTransactionViewModel.cs
// ============================================================================
using IMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace IMS.Application.ViewModels
{
    /// <summary>
    /// Stock transaction view model
    /// </summary>
    public class StockTransactionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Product")]
        public string? ProductName { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        [Display(Name = "Transaction Type")]
        public TransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Remarks are required")]
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Stock Before")]
        public int StockBefore { get; set; }

        [Display(Name = "Stock After")]
        public int StockAfter { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Transaction Date")]
        public DateTime CreatedDate { get; set; }
    }
}

// FILE: IMS.Application/ViewModels/DashboardViewModel.cs
// ============================================================================
using IMS.Domain.Entities;

namespace IMS.Application.ViewModels
{
    /// <summary>
    /// Dashboard statistics view model
    /// </summary>
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int LowStockProductsCount { get; set; }
        public decimal TotalStockValue { get; set; }
        
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
        public List<StockTransaction> RecentTransactions { get; set; } = new List<StockTransaction>();
    }
}
