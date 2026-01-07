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
