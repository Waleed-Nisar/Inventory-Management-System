// ============================================================================
// PART 1: FOUNDATION LAYER (Domain + Identity)
// ============================================================================

// FILE: IMS.Domain/Enums/TransactionType.cs
// ============================================================================
namespace IMS.Domain.Enums
{
    /// <summary>
    /// Represents the type of stock transaction
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Stock added to inventory
        /// </summary>
        StockIn = 1,
        
        /// <summary>
        /// Stock removed from inventory (sale, damage, etc.)
        /// </summary>
        StockOut = 2,
        
        /// <summary>
        /// Stock adjustment (correction, audit)
        /// </summary>
        Adjustment = 3
    }
}

// FILE: IMS.Domain/Enums/UserRole.cs
// ============================================================================
namespace IMS.Domain.Enums
{
    /// <summary>
    /// System roles for authorization
    /// </summary>
    public static class UserRole
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Staff = "Staff";
        public const string Viewer = "Viewer";
        
        /// <summary>
        /// Gets all available roles
        /// </summary>
        public static string[] GetAllRoles() => new[] { Admin, Manager, Staff, Viewer };
    }
}

// FILE: IMS.Domain/Entities/Category.cs
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace IMS.Domain.Entities
{
    /// <summary>
    /// Product category entity
    /// </summary>
    public class Category
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

// FILE: IMS.Domain/Entities/Supplier.cs
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace IMS.Domain.Entities
{
    /// <summary>
    /// Supplier entity for product sourcing
    /// </summary>
    public class Supplier
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? ContactPerson { get; set; }
        
        [StringLength(50)]
        [Phone]
        public string? Phone { get; set; }
        
        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
        
        [StringLength(500)]
        public string? Address { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

// FILE: IMS.Domain/Entities/Product.cs
// ============================================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Domain.Entities
{
    /// <summary>
    /// Product entity representing inventory items
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? SKU { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99)]
        public decimal UnitPrice { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int CurrentStock { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int MinimumStock { get; set; } = 10;
        
        [Required]
        public int CategoryId { get; set; }
        
        public int? SupplierId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; } = null!;
        
        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier? Supplier { get; set; }
        
        public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
        
        /// <summary>
        /// Indicates if stock is below minimum threshold
        /// </summary>
        [NotMapped]
        public bool IsLowStock => CurrentStock < MinimumStock;
    }
}

// FILE: IMS.Domain/Entities/StockTransaction.cs
// ============================================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IMS.Domain.Enums;

namespace IMS.Domain.Entities
{
    /// <summary>
    /// Stock transaction entity for tracking inventory movements
    /// </summary>
    public class StockTransaction
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public TransactionType TransactionType { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;
        
        public int StockBefore { get; set; }
        
        public int StockAfter { get; set; }
        
        [Required]
        [StringLength(256)]
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
    }
}

// FILE: IMS.Infrastructure/Data/ApplicationUser.cs
// ============================================================================
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IMS.Infrastructure.Data
{
    /// <summary>
    /// Extended Identity User with additional properties
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginDate { get; set; }
    }
}
