using IMS.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IMS.Infrastructure.Data
{
    /// <summary>
    /// Application database context with Identity integration
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Category configuration
            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired();
            });

            // Supplier configuration
            builder.Entity<Supplier>(entity =>
            {
                entity.HasIndex(e => e.Name);
                entity.Property(e => e.Name).IsRequired();
            });

            // Product configuration
            builder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.HasIndex(e => e.Name);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // StockTransaction configuration
            builder.Entity<StockTransaction>(entity =>
            {
                entity.HasIndex(e => e.CreatedDate);
                entity.HasIndex(e => e.ProductId);

                entity.HasOne(st => st.Product)
                    .WithMany(p => p.StockTransactions)
                    .HasForeignKey(st => st.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
