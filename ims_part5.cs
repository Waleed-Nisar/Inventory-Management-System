// ============================================================================
// PART 5: PRESENTATION LAYER - FRONTEND (Views + Program.cs)
// ============================================================================

// FILE: IMS.Web/Program.cs
// ============================================================================
using IMS.Application.Interfaces;
using IMS.Application.Services;
using IMS.Domain.Entities;
using IMS.Infrastructure.Data;
using IMS.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Database configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// Register repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStockService, StockService>();

var app = builder.Build();

// ============================================================================
// SMART SEEDING EXECUTION (3-TIER APPROACH)
// ============================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");

        // ========================================================================
        // TIER 1: ALWAYS seed essential data (Roles) - ALL ENVIRONMENTS
        // ========================================================================
        logger.LogInformation("Seeding essential system data (Roles)...");
        await DbSeeder.SeedEssentialDataAsync(roleManager);
        logger.LogInformation("Essential data seeded successfully.");

        // ========================================================================
        // TIER 2: DEBUG MODE ONLY - Seed admin user (COMPILE-TIME CONDITIONAL)
        // ========================================================================
#if DEBUG
        logger.LogInformation("DEBUG MODE: Seeding debug admin user...");
        await DbSeeder.SeedDebugAdminAsync(userManager, roleManager);
        logger.LogInformation("Debug admin user seeded (admin@ims.com / Admin@123).");
#else
        logger.LogInformation("RELEASE MODE: Skipping debug admin user seeding.");
#endif

        // ========================================================================
        // TIER 3: DEVELOPMENT ENVIRONMENT ONLY - Seed test data (RUNTIME CHECK)
        // ========================================================================
        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("DEVELOPMENT ENVIRONMENT: Seeding test data...");
            await TestDataSeeder.SeedAsync(context);
            logger.LogInformation("Test data seeded successfully.");
        }
        else
        {
            logger.LogInformation("PRODUCTION/STAGING: Skipping test data seeding.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
        throw;
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// FILE: IMS.Web/Views/Shared/_Layout.cshtml
// ============================================================================
@using IMS.Domain.Enums
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Inventory Management System</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <header>
        <nav class="navbar navbar-expand-sm navbar-dark bg-dark">
            <div class="container-fluid">
                <a class="navbar-brand" asp-controller="Home" asp-action="Index">
                    <strong>IMS</strong> Inventory
                </a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarNav">
                    <ul class="navbar-nav me-auto">
                        @if (User.Identity?.IsAuthenticated == true)
                        {
                            <li class="nav-item">
                                <a class="nav-link" asp-controller="Home" asp-action="Index">Dashboard</a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" asp-controller="Product" asp-action="Index">Products</a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" asp-controller="Category" asp-action="Index">Categories</a>
                            </li>
                            @if (User.IsInRole(UserRole.Admin) || User.IsInRole(UserRole.Manager) || User.IsInRole(UserRole.Staff))
                            {
                                <li class="nav-item">
                                    <a class="nav-link" asp-controller="Stock" asp-action="Create">Stock Transaction</a>
                                </li>
                            }
                            @if (User.IsInRole(UserRole.Admin))
                            {
                                <li class="nav-item">
                                    <a class="nav-link" asp-controller="Admin" asp-action="Users">Users</a>
                                </li>
                            }
                        }
                    </ul>
                    <partial name="_LoginPartial" />
                </div>
            </div>
        </nav>
    </header>
    <div class="container-fluid mt-4">
        @if (TempData["Success"] != null)
        {
            <div class="alert alert-success alert-dismissible fade show" role="alert">
                @TempData["Success"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        @if (TempData["Error"] != null)
        {
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                @TempData["Error"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        <main role="main" class="pb-3">
            @RenderBody()
        </main>
    </div>

    <footer class="border-top footer text-muted mt-5">
        <div class="container">
            &copy; 2026 - Inventory Management System
        </div>
    </footer>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>

// FILE: IMS.Web/Views/Shared/_LoginPartial.cshtml
// ============================================================================
@using IMS.Infrastructure.Data
@using Microsoft.AspNetCore.Identity
@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser> UserManager

<ul class="navbar-nav">
@if (SignInManager.IsSignedIn(User))
{
    var user = await UserManager.GetUserAsync(User);
    var roles = await UserManager.GetRolesAsync(user!);
    
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle" href="#" id="userDropdown" role="button" data-bs-toggle="dropdown">
            <strong>@user?.FullName</strong> <span class="badge bg-info">@string.Join(", ", roles)</span>
        </a>
        <ul class="dropdown-menu dropdown-menu-end">
            <li><span class="dropdown-item-text">@User.Identity?.Name</span></li>
            <li><hr class="dropdown-divider"></li>
            <li>
                <form asp-controller="Account" asp-action="Logout" method="post" class="d-inline">
                    <button type="submit" class="dropdown-item">Logout</button>
                </form>
            </li>
        </ul>
    </li>
}
else
{
    <li class="nav-item">
        <a class="nav-link" asp-controller="Account" asp-action="Login">Login</a>
    </li>
    <li class="nav-item">
        <a class="nav-link" asp-controller="Account" asp-action="Register">Register</a>
    </li>
}
</ul>

// FILE: IMS.Web/Views/Home/Index.cshtml
// ============================================================================
@model IMS.Application.ViewModels.DashboardViewModel
@{
    ViewData["Title"] = "Dashboard";
}

<div class="row">
    <div class="col-md-12">
        <h2>Dashboard</h2>
        <hr />
    </div>
</div>

<div class="row mb-4">
    <div class="col-md-3">
        <div class="card text-white bg-primary">
            <div class="card-body">
                <h5 class="card-title">Total Products</h5>
                <h2>@Model.TotalProducts</h2>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card text-white bg-success">
            <div class="card-body">
                <h5 class="card-title">Categories</h5>
                <h2>@Model.TotalCategories</h2>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card text-white bg-warning">
            <div class="card-body">
                <h5 class="card-title">Low Stock Items</h5>
                <h2>@Model.LowStockProductsCount</h2>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card text-white bg-info">
            <div class="card-body">
                <h5 class="card-title">Total Stock Value</h5>
                <h2>@Model.TotalStockValue.ToString("C")</h2>
            </div>
        </div>
    </div>
</div>

@if (Model.LowStockProducts.Any())
{
    <div class="row mb-4">
        <div class="col-md-12">
            <div class="card">
                <div class="card-header bg-warning text-dark">
                    <h5 class="mb-0">⚠️ Low Stock Alert</h5>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-sm">
                            <thead>
                                <tr>
                                    <th>Product</th>
                                    <th>Category</th>
                                    <th>Current Stock</th>
                                    <th>Minimum Stock</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                @foreach (var product in Model.LowStockProducts)
                                {
                                    <tr>
                                        <td>@product.Name</td>
                                        <td>@product.Category.Name</td>
                                        <td><span class="badge bg-danger">@product.CurrentStock</span></td>
                                        <td>@product.MinimumStock</td>
                                        <td>
                                            <a asp-controller="Stock" asp-action="Create" asp-route-productId="@product.Id" 
                                               class="btn btn-sm btn-primary">Add Stock</a>
                                        </td>
                                    </tr>
                                }
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
}

<div class="row">
    <div class="col-md-12">
        <div class="card">
            <div class="card-header">
                <h5 class="mb-0">Recent Stock Transactions</h5>
            </div>
            <div class="card-body">
                @if (Model.RecentTransactions.Any())
                {
                    <div class="table-responsive">
                        <table class="table table-hover">
                            <thead>
                                <tr>
                                    <th>Date</th>
                                    <th>Product</th>
                                    <th>Type</th>
                                    <th>Quantity</th>
                                    <th>Stock After</th>
                                    <th>By</th>
                                </tr>
                            </thead>
                            <tbody>
                                @foreach (var transaction in Model.RecentTransactions)
                                {
                                    var badgeClass = transaction.TransactionType == IMS.Domain.Enums.TransactionType.StockIn 
                                        ? "bg-success" : "bg-danger";
                                    <tr>
                                        <td>@transaction.CreatedDate.ToString("g")</td>
                                        <td>@transaction.Product.Name</td>
                                        <td><span class="badge @badgeClass">@transaction.TransactionType</span></td>
                                        <td>@transaction.Quantity</td>
                                        <td>@transaction.StockAfter</td>
                                        <td>@transaction.CreatedBy</td>
                                    </tr>
                                }
                            </tbody>
                        </table>
                    </div>
                }
                else
                {
                    <p class="text-muted">No recent transactions.</p>
                }
            </div>
        </div>
    </div>
</div>

// FILE: IMS.Web/Views/Product/Index.cshtml
// ============================================================================
@model IEnumerable<IMS.Application.ViewModels.ProductViewModel>
@using IMS.Domain.Enums
@{
    ViewData["Title"] = "Products";
}

<div class="row mb-3">
    <div class="col-md-6">
        <h2>Products</h2>
    </div>
    <div class="col-md-6 text-end">
        @if (User.IsInRole(UserRole.Admin) || User.IsInRole(UserRole.Manager))
        {
            <a asp-action="Create" class="btn btn-primary">
                <i class="bi bi-plus-circle"></i> Create New Product
            </a>
        }
    </div>
</div>

<div class="card">
    <div class="card-body">
        <div class="table-responsive">
            <table class="table table-hover">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>SKU</th>
                        <th>Category</th>
                        <th>Price</th>
                        <th>Stock</th>
                        <th>Status</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var item in Model)
                    {
                        <tr class="@(item.IsLowStock ? "table-warning" : "")">
                            <td>
                                <strong>@item.Name</strong>
                                @if (item.IsLowStock)
                                {
                                    <span class="badge bg-warning text-dark ms-2">Low Stock</span>
                                }
                            </td>
                            <td>@item.SKU</td>
                            <td>@item.CategoryName</td>
                            <td>@item.UnitPrice.ToString("C")</td>
                            <td>
                                <span class="badge @(item.IsLowStock ? "bg-danger" : "bg-success")">
                                    @item.CurrentStock
                                </span>
                            </td>
                            <td>
                                <span class="badge @(item.IsActive ? "bg-success" : "bg-secondary")">
                                    @(item.IsActive ? "Active" : "Inactive")
                                </span>
                            </td>
                            <td>
                                <a asp-action="Details" asp-route-id="@item.Id" class="btn btn-sm btn-info">Details</a>
                                @if (User.IsInRole(UserRole.Admin) || User.IsInRole(UserRole.Manager))
                                {
                                    <a asp-action="Edit" asp-route-id="@item.Id" class="btn btn-sm btn-warning">Edit</a>
                                }
                                @if (User.IsInRole(UserRole.Admin))
                                {
                                    <button type="button" class="btn btn-sm btn-danger" data-bs-toggle="modal" 
                                            data-bs-target="#deleteModal@(item.Id)">Delete</button>
                                }
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>
</div>

@if (User.IsInRole(UserRole.Admin))
{
    @foreach (var item in Model)
    {
        <div class="modal fade" id="deleteModal@(item.Id)" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Confirm Delete</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        Are you sure you want to delete <strong>@item.Name</strong>?
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <form asp-action="Delete" asp-route-id="@item.Id" method="post" class="d-inline">
                            @Html.AntiForgeryToken()
                            <button type="submit" class="btn btn-danger">Delete</button>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    }
}

// FILE: IMS.Web/Views/Product/Create.cshtml
// ============================================================================
@model IMS.Application.ViewModels.ProductViewModel
@{
    ViewData["Title"] = "Create Product";
}

<h2>Create Product</h2>
<hr />

<div class="row">
    <div class="col-md-8">
        <form asp-action="Create" method="post">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            
            <div class="mb-3">
                <label asp-for="Name" class="form-label"></label>
                <input asp-for="Name" class="form-control" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="SKU" class="form-label"></label>
                <input asp-for="SKU" class="form-control" />
                <span asp-validation-for="SKU" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="Description" class="form-label"></label>
                <textarea asp-for="Description" class="form-control" rows="3"></textarea>
                <span asp-validation-for="Description" class="text-danger"></span>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="UnitPrice" class="form-label"></label>
                    <input asp-for="UnitPrice" class="form-control" type="number" step="0.01" />
                    <span asp-validation-for="UnitPrice" class="text-danger"></span>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="CategoryId" class="form-label"></label>
                    <select asp-for="CategoryId" class="form-select" asp-items="ViewBag.Categories">
                        <option value="">-- Select Category --</option>
                    </select>
                    <span asp-validation-for="CategoryId" class="text-danger"></span>
                </div>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="CurrentStock" class="form-label"></label>
                    <input asp-for="CurrentStock" class="form-control" type="number" />
                    <span asp-validation-for="CurrentStock" class="text-danger"></span>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="MinimumStock" class="form-label"></label>
                    <input asp-for="MinimumStock" class="form-control" type="number" />
                    <span asp-validation-for="MinimumStock" class="text-danger"></span>
                </div>
            </div>

            <div class="mb-3 form-check">
                <input asp-for="IsActive" class="form-check-input" type="checkbox" />
                <label asp-for="IsActive" class="form-check-label"></label>
            </div>

            <div class="mb-3">
                <button type="submit" class="btn btn-primary">Create</button>
                <a asp-action="Index" class="btn btn-secondary">Cancel</a>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

// FILE: IMS.Web/Views/Product/Edit.cshtml
// ============================================================================
@model IMS.Application.ViewModels.ProductViewModel
@{
    ViewData["Title"] = "Edit Product";
}

<h2>Edit Product</h2>
<hr />

<div class="row">
    <div class="col-md-8">
        <form asp-action="Edit" method="post">
            <input asp-for="Id" type="hidden" />
            <input asp-for="CurrentStock" type="hidden" />
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            
            <div class="mb-3">
                <label asp-for="Name" class="form-label"></label>
                <input asp-for="Name" class="form-control" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="SKU" class="form-label"></label>
                <input asp-for="SKU" class="form-control" />
                <span asp-validation-for="SKU" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="Description" class="form-label"></label>
                <textarea asp-for="Description" class="form-control" rows="3"></textarea>
                <span asp-validation-for="Description" class="text-danger"></span>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="UnitPrice" class="form-label"></label>
                    <input asp-for="UnitPrice" class="form-control" type="number" step="0.01" />
                    <span asp-validation-for="UnitPrice" class="text-danger"></span>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="CategoryId" class="form-label"></label>
                    <select asp-for="CategoryId" class="form-select" asp-items="ViewBag.Categories">
                        <option value="">-- Select Category --</option>
                    </select>
                    <span asp-validation-for="CategoryId" class="text-danger"></span>
                </div>
            </div>

            <div class="mb-3">
                <label asp-for="MinimumStock" class="form-label"></label>
                <input asp-for="MinimumStock" class="form-control" type="number" />
                <span asp-validation-for="MinimumStock" class="text-danger"></span>
                <small class="text-muted">Current Stock: @Model.CurrentStock (use Stock Transaction to modify)</small>
            </div>

            <div class="mb-3 form-check">
                <input asp-for="IsActive" class="form-check-input" type="checkbox" />
                <label asp-for="IsActive" class="form-check-label"></label>
            </div>

            <div class="mb-3">
                <button type="submit" class="btn btn-primary">Save Changes</button>
                <a asp-action="Index" class="btn btn-secondary">Cancel</a>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

// FILE: IMS.Web/Views/Product/Details.cshtml
// ============================================================================
@model IMS.Application.ViewModels.ProductViewModel
@using IMS.Domain.Enums
@{
    ViewData["Title"] = "Product Details";
}

<div class="row mb-3">
    <div class="col-md-6">
        <h2>Product Details</h2>
    </div>
    <div class="col-md-6 text-end">
        @if (User.IsInRole(UserRole.Admin) || User.IsInRole(UserRole.Manager))
        {
            <a asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-warning">Edit</a>
        }
        @if (User.IsInRole(UserRole.Admin) || User.IsInRole(UserRole.Manager) || User.IsInRole(UserRole.Staff))
        {
            <a asp-controller="Stock" asp-action="Create" asp-route-productId="@Model.Id" class="btn btn-primary">Stock Transaction</a>
        }
        <a asp-action="Index" class="btn btn-secondary">Back to List</a>
    </div>
</div>

<div class="row">
    <div class="col-md-12">
        <div class="card">
            <div class="card-header">
                <h4>@Model.Name</h4>
            </div>
            <div class="card-body">
                <dl class="row">
                    <dt class="col-sm-3">SKU</dt>
                    <dd class="col-sm-9">@Model.SKU</dd>

                    <dt class="col-sm-3">Description</dt>
                    <dd class="col-sm-9">@Model.Description</dd>

                    <dt class="col-sm-3">Category</dt>
                    <dd class="col-sm-9">@Model.CategoryName</dd>

                    <dt class="col-sm-3">Unit Price</dt>
                    <dd class="col-sm-9">@Model.UnitPrice.ToString("C")</dd>

                    <dt class="col-sm-3">Current Stock</dt>
                    <dd class="col-sm-9">
                        <span class="badge @(Model.IsLowStock ? "bg-danger" : "bg-success") fs-5">
                            @Model.CurrentStock
                        </span>
                        @if (Model.IsLowStock)
                        {
                            <span class="text-warning ms-2">⚠️ Low Stock Alert</span>
                        }
                    </dd>

                    <dt class="col-sm-3">Minimum Stock</dt>
                    <dd class="col-sm-9">@Model.MinimumStock</dd>

                    <dt class="col-sm-3">Status</dt>
                    <dd class="col-sm-9">
                        <span class="badge @(Model.IsActive ? "bg-success" : "bg-secondary")">
                            @(Model.IsActive ? "Active" : "Inactive")
                        </span>
                    </dd>

                    <dt class="col-sm-3">Created Date</dt>
                    <dd class="col-sm-9">@Model.CreatedDate.ToString("g")</dd>

                    @if (Model.ModifiedDate.HasValue)
                    {
                        <dt class="col-sm-3">Last Modified</dt>
                        <dd class="col-sm-9">@Model.ModifiedDate.Value.ToString("g")</dd>
                    }
                </dl>
            </div>
        </div>
    </div>
</div>

// FILE: IMS.Web/Views/Stock/Create.cshtml
// ============================================================================
@model IMS.Application.ViewModels.StockTransactionViewModel
@{
    ViewData["Title"] = "Stock Transaction";
}

<h2>Create Stock Transaction</h2>
<hr />

<div class="row">
    <div class="col-md-8">
        <form asp-action="Create" method="post">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            
            <div class="mb-3">
                <label asp-for="ProductId" class="form-label"></label>
                <select asp-for="ProductId" class="form-select" asp-items="ViewBag.Products">
                    <option value="">-- Select Product --</option>
                </select>
                <span asp-validation-for="ProductId" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="TransactionType" class="form-label"></label>
                <select asp-for="TransactionType" class="form-select" asp-items="ViewBag.TransactionTypes">
                    <option value="">-- Select Type --</option>
                </select>
                <span asp-validation-for="TransactionType" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="Quantity" class="form-label"></label>
                <input asp-for="Quantity" class="form-control" type="number" />
                <span asp-validation-for="Quantity" class="text-danger"></span>
                <small class="text-muted">For adjustments, enter the new total stock</small>
            </div>

            <div class="mb-3">
                <label asp-for="Remarks" class="form-label"></label>
                <textarea asp-for="Remarks" class="form-control" rows="3"></textarea>
                <span asp-validation-for="Remarks" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <button type="submit" class="btn btn-primary">Create Transaction</button>
                <a asp-controller="Product" asp-action="Index" class="btn btn-secondary">Cancel</a>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

// FILE: IMS.Web/Views/Account/Login.cshtml
// ============================================================================
@model IMS.Web.Controllers.LoginViewModel
@{
    ViewData["Title"] = "Login";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<div class="row justify-content-center mt-5">
    <div class="col-md-6 col-lg-4">
        <div class="card shadow">
            <div class="card-header bg-primary text-white">
                <h4 class="mb-0">Login</h4>
            </div>
            <div class="card-body">
                <form asp-action="Login" method="post">
                    <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>
                    
                    <div class="mb-3">
                        <label asp-for="Email" class="form-label"></label>
                        <input asp-for="Email" class="form-control" autofocus />
                        <span asp-validation-for="Email" class="text-danger"></span>
                    </div>

                    <div class="mb-3">
                        <label asp-for="Password" class="form-label"></label>
                        <input asp-for="Password" class="form-control" type="password" />
                        <span asp-validation-for="Password" class="text-danger"></span>
                    </div>

                    <div class="mb-3 form-check">
                        <input asp-for="RememberMe" class="form-check-input" type="checkbox" />
                        <label asp-for="RememberMe" class="form-check-label"></label>
                    </div>

                    <div class="d-grid">
                        <button type="submit" class="btn btn-primary">Login</button>
                    </div>
                </form>

                <div class="mt-3 text-center">
                    <p>Don't have an account? <a asp-action="Register">Register here</a></p>
                </div>

                <div class="mt-3 alert alert-info">
                    <strong>DEBUG MODE:</strong> admin@ims.com / Admin@123
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

// FILE: IMS.Web/Views/Account/Register.cshtml
// ============================================================================
@model IMS.Web.Controllers.RegisterViewModel
@{
    ViewData["Title"] = "Register";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<div class="row justify-content-center mt-5">
    <div class="col-md-6 col-lg-5">
        <div class="card shadow">
            <div class="card-header bg-success text-white">
                <h4 class="mb-0">Register</h4>
            </div>
            <div class="card-body">
                <form asp-action="Register" method="post">
                    <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>
                    
                    <div class="mb-3">
                        <label asp-for="FullName" class="form-label"></label>
                        <input asp-for="FullName" class="form-control" autofocus />
                        <span asp-validation-for="FullName" class="text-danger"></span>
                    </div>

                    <div class="mb-3">
                        <label asp-for="Email" class="form-label"></label>
                        <input asp-for="Email" class="form-control" />
                        <span asp-validation-for="Email" class="text-danger"></span>
                    </div>

                    <div class="mb-3">
                        <label asp-for="Password" class="form-label"></label>
                        <input asp-for="Password" class="form-control" type="password" />
                        <span asp-validation-for="Password" class="text-danger"></span>
                    </div>

                    <div class="mb-3">
                        <label asp-for="ConfirmPassword" class="form-label"></label>
                        <input asp-for="ConfirmPassword" class="form-control" type="password" />
                        <span asp-validation-for="ConfirmPassword" class="text-danger"></span>
                    </div>

                    <div class="d-grid">
                        <button type="submit" class="btn btn-success">Register</button>
                    </div>
                </form>

                <div class="mt-3 text-center">
                    <p>Already have an account? <a asp-action="Login">Login here</a></p>
                </div>

                <div class="mt-3 alert alert-warning">
                    <small>New users are assigned <strong>Viewer</strong> role by default.</small>
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

// FILE: IMS.Web/Views/Account/AccessDenied.cshtml
// ============================================================================
@{
    ViewData["Title"] = "Access Denied";
}

<div class="row justify-content-center mt-5">
    <div class="col-md-6">
        <div class="card border-danger">
            <div class="card-header bg-danger text-white">
                <h4 class="mb-0">Access Denied</h4>
            </div>
            <div class="card-body text-center">
                <h1 class="display-1 text-danger">🚫</h1>
                <p class="lead">You do not have permission to access this page.</p>
                <p>Contact your administrator if you believe this is an error.</p>
                <a asp-controller="Home" asp-action="Index" class="btn btn-primary mt-3">Go to Dashboard</a>
            </div>
        </div>
    </div>
</div>

// FILE: IMS.Web/Views/Admin/Users.cshtml
// ============================================================================
@model IEnumerable<IMS.Web.Controllers.UserViewModel>
@using IMS.Domain.Enums
@{
    ViewData["Title"] = "User Management";
}

<h2>User Management</h2>
<hr />

<div class="card">
    <div class="card-body">
        <div class="table-responsive">
            <table class="table table-hover">
                <thead>
                    <tr>
                        <th>Full Name</th>
                        <th>Email</th>
                        <th>Roles</th>
                        <th>Status</th>
                        <th>Created</th>
                        <th>Last Login</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var user in Model)
                    {
                        <tr>
                            <td>@user.FullName</td>
                            <td>@user.Email</td>
                            <td><span class="badge bg-info">@user.Roles</span></td>
                            <td>
                                <span class="badge @(user.IsActive ? "bg-success" : "bg-secondary")">
                                    @(user.IsActive ? "Active" : "Inactive")
                                </span>
                            </td>
                            <td>@user.CreatedDate.ToString("d")</td>
                            <td>@(user.LastLoginDate?.ToString("g") ?? "Never")</td>
                            <td>
                                <div class="dropdown">
                                    <button class="btn btn-sm btn-secondary dropdown-toggle" type="button" 
                                            data-bs-toggle="dropdown">
                                        Manage
                                    </button>
                                    <ul class="dropdown-menu">
                                        <li><h6 class="dropdown-header">Change Role</h6></li>
                                        @foreach (var role in UserRole.GetAllRoles())
                                        {
                                            <li>
                                                <form asp-action="ChangeRole" method="post" class="d-inline">
                                                    @Html.AntiForgeryToken()
                                                    <input type="hidden" name="userId" value="@user.Id" />
                                                    <input type="hidden" name="newRole" value="@role" />
                                                    <button type="submit" class="dropdown-item">@role</button>
                                                </form>
                                            </li>
                                        }
                                        <li><hr class="dropdown-divider"></li>
                                        <li>
                                            <form asp-action="ToggleActive" method="post" class="d-inline">
                                                @Html.AntiForgeryToken()
                                                <input type="hidden" name="userId" value="@user.Id" />
                                                <button type="submit" class="dropdown-item">
                                                    @(user.IsActive ? "Deactivate" : "Activate")
                                                </button>
                                            </form>
                                        </li>
                                    </ul>
                                </div>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>
</div>

// FILE: IMS.Web/Views/Category/Index.cshtml
// ============================================================================
@model IEnumerable<IMS.Domain.Entities.Category>
@using IMS.Domain.Enums
@{
    ViewData["Title"] = "Categories";
}

<div class="row mb-3">
    <div class="col-md-6">
        <h2>Categories</h2>
    </div>
    <div class="col-md-6 text-end">
        @if (User.IsInRole(UserRole.Admin) || User.IsInRole(UserRole.Manager))
        {
            <a asp-action="Create" class="btn btn-primary">Create New Category</a>
        }
    </div>
</div>

<div class="card">
    <div class="card-body">
        <div class="table-responsive">
            <table class="table table-hover">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Status</th>
                        <th>Created</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var item in Model)
                    {
                        <tr>
                            <td><strong>@item.Name</strong></td>
                            <td>@item.Description</td>
                            <td>
                                <span class="badge @(item.IsActive ? "bg-success" : "bg-secondary")">
                                    @(item.IsActive ? "Active" : "Inactive")
                                </span>
                            </td>
                            <td>@item.CreatedDate.ToString("d")</td>
                            <td>
                                @if (User.IsInRole(UserRole.Admin) || User.IsInRole(UserRole.Manager))
                                {
                                    <a asp-action="Edit" asp-route-id="@item.Id" class="btn btn-sm btn-warning">Edit</a>
                                }
                                @if (User.IsInRole(UserRole.Admin))
                                {
                                    <button type="button" class="btn btn-sm btn-danger" data-bs-toggle="modal" 
                                            data-bs-target="#deleteModal@(item.Id)">Delete</button>
                                }
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>
</div>

@if (User.IsInRole(UserRole.Admin))
{
    @foreach (var item in Model)
    {
        <div class="modal fade" id="deleteModal@(item.Id)" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Confirm Delete</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        Are you sure you want to delete category <strong>@item.Name</strong>?
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <form asp-action="Delete" asp-route-id="@item.Id" method="post" class="d-inline">
                            @Html.AntiForgeryToken()
                            <button type="submit" class="btn btn-danger">Delete</button>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    }
}

// FILE: IMS.Web/Views/Category/Create.cshtml
// ============================================================================
@model IMS.Domain.Entities.Category
@{
    ViewData["Title"] = "Create Category";
}

<h2>Create Category</h2>
<hr />

<div class="row">
    <div class="col-md-6">
        <form asp-action="Create" method="post">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            
            <div class="mb-3">
                <label asp-for="Name" class="form-label"></label>
                <input asp-for="Name" class="form-control" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="Description" class="form-label"></label>
                <textarea asp-for="Description" class="form-control" rows="3"></textarea>
                <span asp-validation-for="Description" class="text-danger"></span>
            </div>

            <div class="mb-3 form-check">
                <input asp-for="IsActive" class="form-check-input" type="checkbox" checked />
                <label asp-for="IsActive" class="form-check-label"></label>
            </div>

            <div class="mb-3">
                <button type="submit" class="btn btn-primary">Create</button>
                <a asp-action="Index" class="btn btn-secondary">Cancel</a>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

// FILE: IMS.Web/Views/Category/Edit.cshtml
// ============================================================================
@model IMS.Domain.Entities.Category
@{
    ViewData["Title"] = "Edit Category";
}

<h2>Edit Category</h2>
<hr />

<div class="row">
    <div class="col-md-6">
        <form asp-action="Edit" method="post">
            <input asp-for="Id" type="hidden" />
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            
            <div class="mb-3">
                <label asp-for="Name" class="form-label"></label>
                <input asp-for="Name" class="form-control" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="Description" class="form-label"></label>
                <textarea asp-for="Description" class="form-control" rows="3"></textarea>
                <span asp-validation-for="Description" class="text-danger"></span>
            </div>

            <div class="mb-3 form-check">
                <input asp-for="IsActive" class="form-check-input" type="checkbox" />
                <label asp-for="IsActive" class="form-check-label"></label>
            </div>

            <div class="mb-3">
                <button type="submit" class="btn btn-primary">Save Changes</button>
                <a asp-action="Index" class="btn btn-secondary">Cancel</a>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

// FILE: IMS.Web/Views/Shared/_ValidationScriptsPartial.cshtml
// ============================================================================
<script src="https://cdn.jsdelivr.net/npm/jquery@3.6.0/dist/jquery.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/jquery-validation-unobtrusive@4.0.0/dist/jquery.validation.unobtrusive.min.js"></script>

// FILE: IMS.Web/wwwroot/css/site.css
// ============================================================================
html {
  font-size: 14px;
}

@media (min-width: 768px) {
  html {
    font-size: 16px;
  }
}

html {
  position: relative;
  min-height: 100%;
}

body {
  margin-bottom: 60px;
}

.footer {
  position: absolute;
  bottom: 0;
  width: 100%;
  white-space: nowrap;
  line-height: 60px;
}

// FILE: IMS.Web/wwwroot/js/site.js
// ============================================================================
// Site-specific JavaScript