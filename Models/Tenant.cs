using System;

namespace Housing_rental.Models
{
    public class Tenant
    {
        public int TenantId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string NationalId { get; set; }
        public string Address { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
