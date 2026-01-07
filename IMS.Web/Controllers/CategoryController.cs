using IMS.Application.Interfaces;
using IMS.Domain.Entities;
using IMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Web.Controllers
{
    /// <summary>
    /// Category management controller
    /// </summary>
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// List all categories (All roles)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                TempData["Error"] = "Error loading categories.";
                return View(new List<Category>());
            }
        }

        /// <summary>
        /// Create category page (Admin, Manager)
        /// </summary>
        [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
        public IActionResult Create()
        {
            return View(new Category());
        }

        /// <summary>
        /// Create category handler (Admin, Manager)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
        public async Task<IActionResult> Create(Category category)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(category);
                }

                await _categoryService.CreateCategoryAsync(category);
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                ModelState.AddModelError("", "Error creating category: " + ex.Message);
                return View(category);
            }
        }

        /// <summary>
        /// Edit category page (Admin, Manager)
        /// </summary>
        [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category");
                TempData["Error"] = "Error loading category.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Edit category handler (Admin, Manager)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                TempData["Error"] = "Invalid category ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(category);
                }

                var success = await _categoryService.UpdateCategoryAsync(category);
                if (!success)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                ModelState.AddModelError("", "Error updating category: " + ex.Message);
                return View(category);
            }
        }

        /// <summary>
        /// Delete category (Admin only)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (!success)
                {
                    TempData["Error"] = "Category not found.";
                }
                else
                {
                    TempData["Success"] = "Category deleted successfully.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                TempData["Error"] = "Error deleting category.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

