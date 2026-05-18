using System;

namespace AutoTrack.Models
{
    public class Technician
    {
        public int TechnicianID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; }       // joined from Users
        public string Specialization { get; set; }
        public string Level { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Supplier
    {
        public int SupplierID { get; set; }
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PartsSupplied { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

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

    public class Payment
    {
        public int PaymentID { get; set; }
        public string ReceiptNo { get; set; }      // auto-generated e.g. RCP-0001
        public int ServiceID { get; set; }
        public string JobOrderNo { get; set; }     // joined from ServiceRecords
        public string CustomerName { get; set; }   // joined
        public int? ProcessedBy { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? AmountTendered { get; set; }
        public decimal? Change { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    public class RestockRequest
    {
        public int RestockID { get; set; }
        public int PartID { get; set; }
        public string PartName { get; set; }       // joined from Inventory
        public int? SupplierID { get; set; }
        public string SupplierName { get; set; }   // joined from Suppliers
        public int? RequestedBy { get; set; }
        public string RequestedByName { get; set; }
        public int QuantityRequested { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Notes { get; set; }
    }
}
