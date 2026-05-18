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
}
