using System.ComponentModel.DataAnnotations;

namespace FinalGraduationProject.Models
{
    public class Product
    {
        public long Id { get; set; }
        
        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; } = "";
        
        public string Description { get; set; } = "";
        
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = ""; // Men, Women, Kids
        
        [Required(ErrorMessage = "Color is required")]
        public string Color { get; set; } = "";
        
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        public string ImageUrl { get; set; } = "";
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Brand is required")]
        public long? BrandId { get; set; }
        public Brand Brand { get; set; } = null!;

        [Required(ErrorMessage = "Category is required")]
        public long? CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public Inventory? Inventory { get; set; }
        public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
