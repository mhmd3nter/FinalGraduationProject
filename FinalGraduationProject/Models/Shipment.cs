namespace FinalGraduationProject.Models
{
    public class Shipment
    {
        public long Id { get; set; }

        public long OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public long ShippingMethodId { get; set; }
        public ShippingMethod ShippingMethod { get; set; } = null!;

        public string TrackingNumber { get; set; } = "";
        public DateTime ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }
}
