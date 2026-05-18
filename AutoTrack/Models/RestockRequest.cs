using System;

namespace AutoTrack.Models
{
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
