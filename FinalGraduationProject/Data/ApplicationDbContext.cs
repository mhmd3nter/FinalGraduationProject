using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FinalGraduationProject.Models;

namespace FinalGraduationProject.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

        public DbSet<Address> Addresses { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Inventory> Inventories { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Shipment> Shipments { get; set; } = null!;
        public DbSet<ShippingMethod> ShippingMethods { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // جعل الـ IDs Identity
            modelBuilder.Entity<Address>().Property(a => a.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Category>().Property(c => c.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Brand>().Property(b => b.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Product>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Inventory>().Property(i => i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Cart>().Property(c => c.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<CartItem>().Property(ci => ci.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Order>().Property(o => o.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<OrderItem>().Property(oi => oi.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Payment>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Shipment>().Property(s => s.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<ShippingMethod>().Property(sm => sm.Id).ValueGeneratedOnAdd();

            // فهارس وقيود
            modelBuilder.Entity<Product>().HasIndex(p => new { p.Name, p.Size, p.Color });
            modelBuilder.Entity<Inventory>().HasIndex(i => i.ProductId).IsUnique();

            // علاقة 1-1 Product - Inventory
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Inventory)
                .WithOne(i => i.Product)
                .HasForeignKey<Inventory>(i => i.ProductId);

            // علاقة Cart - CartItems
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة Order - OrderItems
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // configure rowversion
            modelBuilder.Entity<Inventory>().Property(i => i.RowVersion).IsRowVersion();

            // Decimal precision configuration
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>().Property(oi => oi.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<ShippingMethod>().Property(sm => sm.Cost).HasPrecision(18, 2);

            // Seeding Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder builder)
        {
            // أدوار
           


            // Brands
            builder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "Nike" },
                new Brand { Id = 2, Name = "Adidas" },
                new Brand { Id = 3, Name = "Puma" }
            );

            // Categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Running" },
                new Category { Id = 2, Name = "Casual" },
                new Category { Id = 3, Name = "Sports" }
            );

            // Products (20 منتج)
            builder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Nike Air Zoom", BrandId = 1, CategoryId = 1, Description = "Running shoes", Gender = "Men", Size = 42, Color = "Black", Price = 120, ImageUrl = "/images/nike_airzoom.jpg", IsActive = true },
                new Product { Id = 2, Name = "Nike Revolution", BrandId = 1, CategoryId = 1, Description = "Lightweight running shoes", Gender = "Women", Size = 38, Color = "Blue", Price = 90, ImageUrl = "/images/nike_revolution.jpg", IsActive = true },
                new Product { Id = 3, Name = "Adidas Ultraboost", BrandId = 2, CategoryId = 1, Description = "High comfort running shoes", Gender = "Men", Size = 43, Color = "White", Price = 150, ImageUrl = "/images/adidas_ultraboost.jpg", IsActive = true },
                new Product { Id = 4, Name = "Adidas Stan Smith", BrandId = 2, CategoryId = 2, Description = "Classic casual shoes", Gender = "Unisex", Size = 41, Color = "Green", Price = 100, ImageUrl = "/images/adidas_stansmith.jpg", IsActive = true },
                new Product { Id = 5, Name = "Puma Smash", BrandId = 3, CategoryId = 2, Description = "Casual sneakers", Gender = "Women", Size = 39, Color = "Pink", Price = 85, ImageUrl = "/images/puma_smash.jpg", IsActive = true },
                new Product { Id = 6, Name = "Nike Pegasus", BrandId = 1, CategoryId = 1, Description = "Versatile running shoes", Gender = "Men", Size = 44, Color = "Grey", Price = 130, ImageUrl = "/images/nike_pegasus.jpg", IsActive = true },
                new Product { Id = 7, Name = "Adidas Gazelle", BrandId = 2, CategoryId = 2, Description = "Retro casual shoes", Gender = "Unisex", Size = 40, Color = "Red", Price = 95, ImageUrl = "/images/adidas_gazelle.jpg", IsActive = true },
                new Product { Id = 8, Name = "Puma Future Rider", BrandId = 3, CategoryId = 3, Description = "Sporty sneakers", Gender = "Men", Size = 42, Color = "Blue/White", Price = 110, ImageUrl = "/images/puma_future.jpg", IsActive = true },
                new Product { Id = 9, Name = "Nike Court Vision", BrandId = 1, CategoryId = 2, Description = "Casual lifestyle shoes", Gender = "Men", Size = 43, Color = "White/Black", Price = 105, ImageUrl = "/images/nike_courtvision.jpg", IsActive = true },
                new Product { Id = 10, Name = "Adidas Superstar", BrandId = 2, CategoryId = 2, Description = "Street style classic", Gender = "Women", Size = 37, Color = "White/Gold", Price = 110, ImageUrl = "/images/adidas_superstar.jpg", IsActive = true },
                new Product { Id = 11, Name = "Puma RS-X", BrandId = 3, CategoryId = 3, Description = "Chunky sneakers", Gender = "Unisex", Size = 42, Color = "Black/Orange", Price = 125, ImageUrl = "/images/puma_rsx.jpg", IsActive = true },
                new Product { Id = 12, Name = "Nike Air Max", BrandId = 1, CategoryId = 3, Description = "Sporty lifestyle shoes", Gender = "Women", Size = 39, Color = "Pink/White", Price = 140, ImageUrl = "/images/nike_airmax.jpg", IsActive = true },
                new Product { Id = 13, Name = "Adidas NMD", BrandId = 2, CategoryId = 3, Description = "Trendy sports shoes", Gender = "Men", Size = 44, Color = "Black", Price = 160, ImageUrl = "/images/adidas_nmd.jpg", IsActive = true },
                new Product { Id = 14, Name = "Puma Cali", BrandId = 3, CategoryId = 2, Description = "Casual sneakers", Gender = "Women", Size = 38, Color = "White", Price = 90, ImageUrl = "/images/puma_cali.jpg", IsActive = true },
                new Product { Id = 15, Name = "Nike Downshifter", BrandId = 1, CategoryId = 1, Description = "Budget running shoes", Gender = "Men", Size = 42, Color = "Grey", Price = 75, ImageUrl = "/images/nike_downshifter.jpg", IsActive = true },
                new Product { Id = 16, Name = "Adidas ZX Flux", BrandId = 2, CategoryId = 3, Description = "Sporty sneakers", Gender = "Unisex", Size = 41, Color = "Blue", Price = 115, ImageUrl = "/images/adidas_zxflux.jpg", IsActive = true },
                new Product { Id = 17, Name = "Puma Ignite", BrandId = 3, CategoryId = 1, Description = "Performance running shoes", Gender = "Men", Size = 43, Color = "Black/Red", Price = 135, ImageUrl = "/images/puma_ignite.jpg", IsActive = true },
                new Product { Id = 18, Name = "Nike Blazer", BrandId = 1, CategoryId = 2, Description = "Classic sneakers", Gender = "Unisex", Size = 42, Color = "White", Price = 95, ImageUrl = "/images/nike_blazer.jpg", IsActive = true },
                new Product { Id = 19, Name = "Adidas Terrex", BrandId = 2, CategoryId = 3, Description = "Outdoor trail shoes", Gender = "Men", Size = 45, Color = "Brown", Price = 170, ImageUrl = "/images/adidas_terrex.jpg", IsActive = true },
                new Product { Id = 20, Name = "Puma Enzo", BrandId = 3, CategoryId = 3, Description = "Training shoes", Gender = "Women", Size = 38, Color = "Purple", Price = 100, ImageUrl = "/images/puma_enzo.jpg", IsActive = true }
            );

            // Inventory لكل منتج (تاريخ ثابت بدل Dynamic)
            builder.Entity<Inventory>().HasData(
                Enumerable.Range(1, 20).Select(i =>
                    new Inventory
                    {
                        Id = i,
                        ProductId = i,
                        QuantityAvailable = 50,
                        QuantityReserved = 0,
                        SafetyStockThreshold = 5,
                        LastStockChangeAt = new DateTime(2025, 1, 1)
                    }
                )
            );
        }
    }
}
