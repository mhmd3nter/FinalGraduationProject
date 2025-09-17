namespace FinalGraduationProject.Models
{
    public class ShippingMethod
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Cost { get; set; }
        public int EstimatedDays { get; set; }

        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}
