namespace Housing_rental.Models
{
    public static class PaymentStatuses
    {
        public const string Posted = "Posted";
        public const string Reversed = "Reversed";
    }

    public static class ChargeStatuses
    {
        public const string All = "All";
        public const string Due = "Due";
        public const string Partial = "Partial";
        public const string Paid = "Paid";
        public const string Overdue = "Overdue";
        public const string Waived = "Waived";
    }

    public static class PaymentMethods
    {
        public const string Cash = "Cash";
        public const string BankTransfer = "BankTransfer";
        public const string Card = "Card";
        public const string MobileBanking = "MobileBanking";
        public const string Cheque = "Cheque";

        public static readonly string[] All =
        {
            Cash,
            BankTransfer,
            Card,
            MobileBanking,
            Cheque
        };

        public static bool IsValid(string value)
        {
            foreach (string method in All)
            {
                if (method == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
