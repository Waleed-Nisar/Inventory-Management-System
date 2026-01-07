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
