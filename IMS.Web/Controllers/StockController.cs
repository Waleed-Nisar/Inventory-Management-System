using IMS.Application.Interfaces;
using IMS.Application.ViewModels;
using IMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Web.Controllers
{
    /// <summary>
    /// Stock transaction controller
    /// </summary>
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager},{UserRole.Staff}")]
    public class StockController : Controller
    {
        private readonly IStockService _stockService;
        private readonly IProductService _productService;
        private readonly ILogger<StockController> _logger;

        public StockController(
            IStockService stockService,
            IProductService productService,
            ILogger<StockController> logger)
        {
            _stockService = stockService;
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Create stock transaction page
        /// </summary>
        public async Task<IActionResult> Create(int? productId)
        {
            var model = new StockTransactionViewModel();

            if (productId.HasValue)
            {
                model.ProductId = productId.Value;
            }

            await PopulateProductsDropdown(productId);
            PopulateTransactionTypes();

            return View(model);
        }

        /// <summary>
        /// Create stock transaction handler
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockTransactionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await PopulateProductsDropdown(model.ProductId);
                    PopulateTransactionTypes();
                    return View(model);
                }

                var userName = User.Identity?.Name ?? "Unknown";
                await _stockService.CreateTransactionAsync(model, userName);

                TempData["Success"] = "Stock transaction completed successfully.";
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateProductsDropdown(model.ProductId);
                PopulateTransactionTypes();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stock transaction");
                ModelState.AddModelError("", "Error creating transaction: " + ex.Message);
                await PopulateProductsDropdown(model.ProductId);
                PopulateTransactionTypes();
                return View(model);
            }
        }

        /// <summary>
        /// View transaction history for a product
        /// </summary>
        [Authorize]
        public async Task<IActionResult> History(int productId)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index", "Product");
                }

                var transactions = await _stockService.GetTransactionsByProductAsync(productId);
                ViewBag.ProductName = product.Name;

                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transaction history");
                TempData["Error"] = "Error loading transaction history.";
                return RedirectToAction("Index", "Product");
            }
        }

        private async Task PopulateProductsDropdown(int? selectedProductId = null)
        {
            var products = await _productService.GetAllProductsAsync();
            ViewBag.Products = new SelectList(
                products.Where(p => p.IsActive),
                "Id",
                "Name",
                selectedProductId);
        }

        private void PopulateTransactionTypes()
        {
            ViewBag.TransactionTypes = new SelectList(
                Enum.GetValues(typeof(TransactionType))
                    .Cast<TransactionType>()
                    .Select(t => new { Value = (int)t, Text = t.ToString() }),
                "Value",
                "Text");
        }
    }
}
    