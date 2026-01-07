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
