namespace FinalGraduationProject.Models
{
    public class Payment
    {
        public long Id { get; set; }

        public long OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string Method { get; set; } = ""; // CreditCard, PayPal, COD
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    }
}
