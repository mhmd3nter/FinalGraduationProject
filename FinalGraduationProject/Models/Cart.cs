namespace FinalGraduationProject.Models
{
    public class Cart
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // 🆕 Helpful properties for cart calculations
        public decimal Subtotal => CartItems.Sum(item => item.TotalPrice);
        public decimal Tax => Subtotal * 0.14m; // 14% tax (you can change this)
        public decimal Shipping => Subtotal > 100 ? 0 : 10; // Free shipping over $100
        public decimal Total => Subtotal + Tax + Shipping;
        public int TotalItems => CartItems.Sum(item => item.Quantity);
    }
}
