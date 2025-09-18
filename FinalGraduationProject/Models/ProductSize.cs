namespace FinalGraduationProject.Models
{
    public class ProductSize
    {
        public long Id { get; set; }          // surrogate PK
        public long ProductId { get; set; }   // يتطابق مع Product.Id (long)
        public Product Product { get; set; } = null!;

        public int SizeId { get; set; }       // يتطابق مع Size.Id (int)
        public Size Size { get; set; } = null!;

        public int Quantity { get; set; }     // الكمية المتوفرة للمقاس هذا
    }


}
