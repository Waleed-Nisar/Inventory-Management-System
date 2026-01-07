using IMS.Application.ViewModels;

namespace IMS.Application.Interfaces
{
    /// <summary>
    /// Product service interface
    /// </summary>
    public interface IProductService
    {
        Task<IEnumerable<ProductViewModel>> GetAllProductsAsync();
        Task<ProductViewModel?> GetProductByIdAsync(int id);
        Task<ProductViewModel> CreateProductAsync(ProductViewModel model);
        Task<bool> UpdateProductAsync(ProductViewModel model);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<ProductViewModel>> GetLowStockProductsAsync();
        Task<bool> ProductExistsAsync(int id);
        Task<bool> SKUExistsAsync(string sku, int? excludeId = null);
    }
}
