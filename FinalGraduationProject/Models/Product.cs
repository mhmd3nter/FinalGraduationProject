namespace FinalGraduationProject.Models
{
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Gender { get; set; } = ""; // Men, Women, Kids
        public string Color { get; set; } = "";
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = "";
        public bool IsActive { get; set; } = true;


        public long? BrandId { get; set; }
        public Brand Brand { get; set; } = null!;

        public long? CategoryId { get; set; }
        public Category Category { get; set; } = null!;

       // public int Size { get; set; }

        public Inventory? Inventory { get; set; }
        public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
