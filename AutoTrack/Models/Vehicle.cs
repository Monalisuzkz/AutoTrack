using System;

namespace AutoTrack.Models
{
    public class Vehicle
    {
        public int VehicleID { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }   // joined from Customers
        public string PlateNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public string EngineType { get; set; }
        public string Transmission { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Helper property
        public string DisplayName => $"{Year} {Make} {Model} ({PlateNumber})";
    }
}
