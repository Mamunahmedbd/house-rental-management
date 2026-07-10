using System;

namespace Housing_rental.Models
{
    public class House
    {
        public int HouseId { get; set; }
        public int PropertyId { get; set; }
        public string HouseName { get; set; }
        public string FloorNo { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
