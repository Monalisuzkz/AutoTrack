using System;

namespace AutoTrack.Models
{
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
}
