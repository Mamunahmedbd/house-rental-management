using System;

namespace Housing_rental.Models
{
    public class Expense
    {
        public int ExpenseId { get; set; }
        public int? PropertyId { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
