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
