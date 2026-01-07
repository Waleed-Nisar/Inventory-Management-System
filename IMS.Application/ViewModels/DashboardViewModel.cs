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
