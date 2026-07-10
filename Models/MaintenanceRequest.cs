using System;

namespace Housing_rental.Models
{
    public class MaintenanceRequest
    {
        public int MaintenanceId { get; set; }
        public int? PropertyId { get; set; }
        public int? RoomId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
