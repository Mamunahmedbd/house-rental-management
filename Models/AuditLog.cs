using System;

namespace Housing_rental.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public int? UserId { get; set; }
        public string ActionName { get; set; }
        public string TableName { get; set; }
        public string RecordId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
