using System;

namespace AutoTrack.Models
{
    public class ServiceRecord
    {
        public int ServiceID { get; set; }
        public string JobOrderNo { get; set; }     // auto-generated e.g. JO-0001
        public int VehicleID { get; set; }
        public string PlateNumber { get; set; }    // joined from Vehicles
        public string VehicleName { get; set; }    // joined — Make + Model
        public int? TechnicianID { get; set; }
        public string TechnicianName { get; set; } // joined from Users
        public int? AssignedBy { get; set; }
        public string ServiceType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime DateIn { get; set; }
        public DateTime? EstimatedDate { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
