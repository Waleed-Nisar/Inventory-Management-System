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