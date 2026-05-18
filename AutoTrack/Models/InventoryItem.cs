using System;

namespace AutoTrack.Models
{
    public class InventoryItem
    {
        public int PartID { get; set; }
        public int? SupplierID { get; set; }
        public string SupplierName { get; set; }   // joined from Suppliers
        public string PartName { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Helper — stock status
        public string StockStatus
        {
            get
            {
                if (Quantity <= 0) return "Out of Stock";
                if (Quantity <= ReorderLevel) return "Low Stock";
                return "In Stock";
            }
        }
    }
}
