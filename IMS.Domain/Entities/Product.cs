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
