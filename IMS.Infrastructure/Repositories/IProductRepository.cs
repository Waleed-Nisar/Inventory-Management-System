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