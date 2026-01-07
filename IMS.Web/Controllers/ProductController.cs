using IMS.Application.Interfaces;
using IMS.Application.ViewModels;
using IMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Web.Controllers
{
    /// <summary>
    /// Product management controller with role-based access
    /// </summary>
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// List all products (All roles)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                TempData["Error"] = "Error loading products.";
                return View(new List<ProductViewModel>());
            }
        }

        /// <summary>
        /// Product details (All roles)
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details");
                TempData["Error"] = "Error loading product details.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Create product page (Admin, Manager)
        /// </summary>
        [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropdown();
            return View(new ProductViewModel());
        }

        /// <summary>
        /// Create product handler (Admin, Manager)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            try
            {
                // Validate SKU uniqueness
                if (!string.IsNullOrWhiteSpace(model.SKU) && await _productService.SKUExistsAsync(model.SKU))
                {
                    ModelState.AddModelError("SKU", "SKU already exists.");
                }

                if (!ModelState.IsValid)
                {
                    await PopulateCategoriesDropdown(model.CategoryId);
                    return View(model);
                }

                await _productService.CreateProductAsync(model);
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", "Error creating product: " + ex.Message);
                await PopulateCategoriesDropdown(model.CategoryId);
                return View(model);
            }
        }

        /// <summary>
        /// Edit product page (Admin, Manager)
        /// </summary>
        [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                await PopulateCategoriesDropdown(product.CategoryId);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for edit");
                TempData["Error"] = "Error loading product.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Edit product handler (Admin, Manager)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.Id)
            {
                TempData["Error"] = "Invalid product ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Validate SKU uniqueness
                if (!string.IsNullOrWhiteSpace(model.SKU) && await _productService.SKUExistsAsync(model.SKU, model.Id))
                {
                    ModelState.AddModelError("SKU", "SKU already exists.");
                }

                if (!ModelState.IsValid)
                {
                    await PopulateCategoriesDropdown(model.CategoryId);
                    return View(model);
                }

                var success = await _productService.UpdateProductAsync(model);
                if (!success)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                ModelState.AddModelError("", "Error updating product: " + ex.Message);
                await PopulateCategoriesDropdown(model.CategoryId);
                return View(model);
            }
        }

        /// <summary>
        /// Delete product (Admin only)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _productService.DeleteProductAsync(id);
                if (!success)
                {
                    TempData["Error"] = "Product not found.";
                }
                else
                {
                    TempData["Success"] = "Product deleted successfully.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                TempData["Error"] = "Error deleting product.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesDropdown(int? selectedCategoryId = null)
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
        }
    }
}
