namespace FinalGraduationProject.Models
{
    public class Order
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
        public decimal TotalAmount { get; set; }

        // Add these lines:
        public long? AddressId { get; set; }   // مفتاح أجنبي
        public Address Address { get; set; } = null!; // العلاقة


        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

        // Add this property to the Order class
        public string? PaymentMethod { get; set; }

        // Add this property if it does not already exist
        public string? CancellationReason { get; set; }
    }
}
