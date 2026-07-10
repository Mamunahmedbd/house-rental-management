using System;

namespace Housing_rental.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public int HouseId { get; set; }
        public string RoomNo { get; set; }
        public string RoomType { get; set; }
        public decimal MonthlyRent { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
