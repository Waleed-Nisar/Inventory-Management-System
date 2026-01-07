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
