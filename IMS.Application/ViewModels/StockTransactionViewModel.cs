using IMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace IMS.Application.ViewModels
{
    /// <summary>
    /// Stock transaction view model
    /// </summary>
    public class StockTransactionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Product")]
        public string? ProductName { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        [Display(Name = "Transaction Type")]
        public TransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Remarks are required")]
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Stock Before")]
        public int StockBefore { get; set; }

        [Display(Name = "Stock After")]
        public int StockAfter { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Transaction Date")]
        public DateTime CreatedDate { get; set; }
    }
}
