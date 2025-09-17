namespace FinalGraduationProject.Models
{
    public class CartItem
    {
        public long Id { get; set; }

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public long CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        public int Quantity { get; set; } // إضافة هذا الحقل لتحديد الكمية

        // خاصية لحساب سعر العنصر الإجمالي
        public decimal TotalPrice => Product.Price * Quantity;
    }
}