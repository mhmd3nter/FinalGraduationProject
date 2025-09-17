using System.ComponentModel.DataAnnotations;

namespace FinalGraduationProject.Models
{
    public class Inventory
    {
        public long Id { get; set; }

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int QuantityAvailable { get; set; }
        public int QuantityReserved { get; set; }
        public int SafetyStockThreshold { get; set; }
        public DateTime LastStockChangeAt { get; set; }

        [Timestamp] // RowVersion
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
