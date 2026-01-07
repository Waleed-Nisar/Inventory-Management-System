// ============================================================================
// PART 4: PRESENTATION LAYER - BACKEND (Controllers with RBAC)
// ============================================================================

// FILE: IMS.Web/Controllers/HomeController.cs
// ============================================================================
using IMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Web.Controllers
{
    /// <summary>
    /// Home controller for dashboard and landing page
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IStockService _stockService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IStockService stockService, ILogger<HomeController> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard with inventory statistics
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await _stockService.GetDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["Error"] = "Error loading dashboard data.";
                return View();
            }
        }

        /// <summary>
        /// Privacy policy page
        /// </summary>
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Error handler
        /// </summary>
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}

// FILE: IMS.Web/Controllers/ProductController.cs
// ============================================================================
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

// FILE: IMS.Web/Controllers/CategoryController.cs
// ============================================================================
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

// FILE: IMS.Web/Controllers/StockController.cs
// ============================================================================
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

// FILE: IMS.Web/Controllers/AccountController.cs
// ============================================================================
using IMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Controllers
{
    /// <summary>
    /// Authentication controller
    /// </summary>
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Login page
        /// </summary>
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Login handler
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                
                // Update last login date
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    user.LastLoginDate = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }

                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        /// <summary>
        /// Register page
        /// </summary>
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Register handler
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account.");

                // Assign default Viewer role to new users
                await _userManager.AddToRoleAsync(user, "Viewer");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        /// <summary>
        /// Logout handler
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Access denied page
        /// </summary>
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }

    // View Models for Account
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

// FILE: IMS.Web/Controllers/AdminController.cs
// ============================================================================
using IMS.Domain.Enums;
using IMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Web.Controllers
{
    /// <summary>
    /// Admin user management controller
    /// </summary>
    [Authorize(Roles = UserRole.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// List all users with their roles
        /// </summary>
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userViewModels = new List<UserViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userViewModels.Add(new UserViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email ?? string.Empty,
                        IsActive = user.IsActive,
                        CreatedDate = user.CreatedDate,
                        LastLoginDate = user.LastLoginDate,
                        Roles = string.Join(", ", roles)
                    });
                }

                return View(userViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["Error"] = "Error loading users.";
                return View(new List<UserViewModel>());
            }
        }

        /// <summary>
        /// Change user role
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Users));
                }

                // Remove all existing roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Add new role
                await _userManager.AddToRoleAsync(user, newRole);

                TempData["Success"] = $"User role changed to {newRole}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing user role");
                TempData["Error"] = "Error changing user role.";
            }

            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Toggle user active status
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Users));
                }

                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);

                TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status");
                TempData["Error"] = "Error updating user status.";
            }

            return RedirectToAction(nameof(Users));
        }
    }

    // View Model for Admin Users page
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string Roles { get; set; } = string.Empty;
    }
}
