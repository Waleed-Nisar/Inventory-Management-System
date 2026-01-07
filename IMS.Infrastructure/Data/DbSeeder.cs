using IMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace IMS.Infrastructure.Data
{
    /// <summary>
    /// TIER 1: Critical system data seeder (ALWAYS runs)
    /// Seeds essential data required for the application to function
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>
        /// Seeds essential system data (Roles) - runs in ALL environments
        /// </summary>
        public static async Task SeedEssentialDataAsync(RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles (Required for authorization)
            foreach (var roleName in UserRole.GetAllRoles())
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

#if DEBUG
        /// <summary>
        /// TIER 2: Seeds debug admin user - ONLY in DEBUG builds
        /// This method is completely excluded from Release builds
        /// </summary>
        public static async Task SeedDebugAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            const string adminEmail = "admin@ims.com";
            const string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRole.Admin);
                }
            }
        }
#endif
    }
}
