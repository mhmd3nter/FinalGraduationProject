using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
        public DbSet<Size> Sizes { get; set; } = null!;
        public DbSet<ProductSize> ProductSizes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // IDs auto-increment
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
            modelBuilder.Entity<Size>().Property(s => s.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<ProductSize>().Property(ps => ps.Id).ValueGeneratedOnAdd();

            // Indexes
            modelBuilder.Entity<Product>().HasIndex(p => new { p.Name, p.Color });
            modelBuilder.Entity<Inventory>().HasIndex(i => i.ProductId).IsUnique();

            // Product - Inventory (1:1)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Inventory)
                .WithOne(i => i.Product)
                .HasForeignKey<Inventory>(i => i.ProductId);

            // Product - ProductSize (1:M)
            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductSizes)
                .WithOne(ps => ps.Product)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductSize - Size (M:1)
            modelBuilder.Entity<ProductSize>()
                .HasOne(ps => ps.Size)
                .WithMany()
                .HasForeignKey(ps => ps.SizeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cart - CartItems
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem -> ProductSize
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductSize)
                .WithMany()
                .HasForeignKey(ci => ci.ProductSizeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order - OrderItems
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> ProductSize
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductSize)
                .WithMany()
                .HasForeignKey(oi => oi.ProductSizeId)
                .OnDelete(DeleteBehavior.Restrict);

            // RowVersion
            modelBuilder.Entity<Inventory>().Property(i => i.RowVersion).IsRowVersion();

            // Decimal precision
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>().Property(oi => oi.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<ShippingMethod>().Property(sm => sm.Cost).HasPrecision(18, 2);

            // Seed Sizes
            modelBuilder.Entity<Size>().HasData(
                new Size { Id = 1, Name = "37" },
                new Size { Id = 2, Name = "38" },
                new Size { Id = 3, Name = "39" },
                new Size { Id = 4, Name = "40" },
                new Size { Id = 5, Name = "41" },
                new Size { Id = 6, Name = "42" },
                new Size { Id = 7, Name = "43" },
                new Size { Id = 8, Name = "44" },
                new Size { Id = 9, Name = "45" }
            );

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder builder)
        {
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

            // Products (20)
            builder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Nike Air Zoom", BrandId = 1, CategoryId = 1, Description = "Running shoes", Gender = "Men", Color = "Black", Price = 120m, ImageUrl = "/images/nike_airzoom.jpg", IsActive = true },
                new Product { Id = 2, Name = "Nike Revolution", BrandId = 1, CategoryId = 1, Description = "Lightweight running shoes", Gender = "Women", Color = "Blue", Price = 90m, ImageUrl = "/images/nike_revolution.jpg", IsActive = true },
                new Product { Id = 3, Name = "Adidas Ultraboost", BrandId = 2, CategoryId = 1, Description = "High comfort running shoes", Gender = "Men", Color = "White", Price = 150m, ImageUrl = "/images/adidas_ultraboost.jpg", IsActive = true },
                new Product { Id = 4, Name = "Adidas Stan Smith", BrandId = 2, CategoryId = 2, Description = "Classic casual shoes", Gender = "Unisex", Color = "Green", Price = 100m, ImageUrl = "/images/adidas_stansmith.jpg", IsActive = true },
                new Product { Id = 5, Name = "Puma Smash", BrandId = 3, CategoryId = 2, Description = "Casual sneakers", Gender = "Women", Color = "Pink", Price = 85m, ImageUrl = "/images/puma_smash.jpg", IsActive = true },
                new Product { Id = 6, Name = "Nike Pegasus", BrandId = 1, CategoryId = 1, Description = "Versatile running shoes", Gender = "Men", Color = "Grey", Price = 130m, ImageUrl = "/images/nike_pegasus.jpg", IsActive = true },
                new Product { Id = 7, Name = "Adidas Gazelle", BrandId = 2, CategoryId = 2, Description = "Retro casual shoes", Gender = "Unisex", Color = "Red", Price = 95m, ImageUrl = "/images/adidas_gazelle.jpg", IsActive = true },
                new Product { Id = 8, Name = "Puma Future Rider", BrandId = 3, CategoryId = 3, Description = "Sporty sneakers", Gender = "Men", Color = "Blue/White", Price = 110m, ImageUrl = "/images/puma_future.jpg", IsActive = true },
                new Product { Id = 9, Name = "Nike Court Vision", BrandId = 1, CategoryId = 2, Description = "Casual lifestyle shoes", Gender = "Men", Color = "White/Black", Price = 105m, ImageUrl = "/images/nike_courtvision.jpg", IsActive = true },
                new Product { Id = 10, Name = "Adidas Superstar", BrandId = 2, CategoryId = 2, Description = "Street style classic", Gender = "Women", Color = "White/Gold", Price = 110m, ImageUrl = "/images/adidas_superstar.jpg", IsActive = true },
                new Product { Id = 11, Name = "Puma RS-X", BrandId = 3, CategoryId = 3, Description = "Chunky sneakers", Gender = "Unisex", Color = "Black/Orange", Price = 125m, ImageUrl = "/images/puma_rsx.jpg", IsActive = true },
                new Product { Id = 12, Name = "Nike Air Max", BrandId = 1, CategoryId = 3, Description = "Sporty lifestyle shoes", Gender = "Women", Color = "Pink/White", Price = 140m, ImageUrl = "/images/nike_airmax.jpg", IsActive = true },
                new Product { Id = 13, Name = "Adidas NMD", BrandId = 2, CategoryId = 3, Description = "Trendy sports shoes", Gender = "Men", Color = "Black", Price = 160m, ImageUrl = "/images/adidas_nmd.jpg", IsActive = true },
                new Product { Id = 14, Name = "Puma Cali", BrandId = 3, CategoryId = 2, Description = "Casual sneakers", Gender = "Women", Color = "White", Price = 90m, ImageUrl = "/images/puma_cali.jpg", IsActive = true },
                new Product { Id = 15, Name = "Nike Downshifter", BrandId = 1, CategoryId = 1, Description = "Budget running shoes", Gender = "Men", Color = "Grey", Price = 75m, ImageUrl = "/images/nike_downshifter.jpg", IsActive = true },
                new Product { Id = 16, Name = "Adidas ZX Flux", BrandId = 2, CategoryId = 3, Description = "Sporty sneakers", Gender = "Unisex", Color = "Blue", Price = 115m, ImageUrl = "/images/adidas_zxflux.jpg", IsActive = true },
                new Product { Id = 17, Name = "Puma Ignite", BrandId = 3, CategoryId = 1, Description = "Performance running shoes", Gender = "Men", Color = "Black/Red", Price = 135m, ImageUrl = "/images/puma_ignite.jpg", IsActive = true },
                new Product { Id = 18, Name = "Nike Blazer", BrandId = 1, CategoryId = 2, Description = "Classic sneakers", Gender = "Unisex", Color = "White", Price = 95m, ImageUrl = "/images/nike_blazer.jpg", IsActive = true },
                new Product { Id = 19, Name = "Adidas Terrex", BrandId = 2, CategoryId = 3, Description = "Outdoor trail shoes", Gender = "Men", Color = "Brown", Price = 170m, ImageUrl = "/images/adidas_terrex.jpg", IsActive = true },
                new Product { Id = 20, Name = "Puma Enzo", BrandId = 3, CategoryId = 3, Description = "Training shoes", Gender = "Women", Color = "Purple", Price = 100m, ImageUrl = "/images/puma_enzo.jpg", IsActive = true }
            );

            builder.Entity<ProductSize>().HasData(
                // Product 1 (Id 1) -> total 24
                new ProductSize { Id = 1, ProductId = 1, SizeId = 5, Quantity = 10 },
                new ProductSize { Id = 2, ProductId = 1, SizeId = 6, Quantity = 8 },
                new ProductSize { Id = 3, ProductId = 1, SizeId = 7, Quantity = 6 },

                // Product 2 -> total 26
                new ProductSize { Id = 4, ProductId = 2, SizeId = 1, Quantity = 10 },
                new ProductSize { Id = 5, ProductId = 2, SizeId = 2, Quantity = 9 },
                new ProductSize { Id = 6, ProductId = 2, SizeId = 3, Quantity = 7 },

                // Product 3 -> total 33
                new ProductSize { Id = 7, ProductId = 3, SizeId = 6, Quantity = 12 },
                new ProductSize { Id = 8, ProductId = 3, SizeId = 7, Quantity = 11 },
                new ProductSize { Id = 9, ProductId = 3, SizeId = 8, Quantity = 10 },

                // Product 4 -> total 18
                new ProductSize { Id = 10, ProductId = 4, SizeId = 6, Quantity = 6 },
                new ProductSize { Id = 11, ProductId = 4, SizeId = 5, Quantity = 6 },
                new ProductSize { Id = 12, ProductId = 4, SizeId = 7, Quantity = 6 },

                // Product 5 -> total 25
                new ProductSize { Id = 13, ProductId = 5, SizeId = 2, Quantity = 10 },
                new ProductSize { Id = 14, ProductId = 5, SizeId = 3, Quantity = 9 },
                new ProductSize { Id = 15, ProductId = 5, SizeId = 4, Quantity = 6 },

                // Product 6 -> total 28
                new ProductSize { Id = 16, ProductId = 6, SizeId = 6, Quantity = 10 },
                new ProductSize { Id = 17, ProductId = 6, SizeId = 7, Quantity = 9 },
                new ProductSize { Id = 18, ProductId = 6, SizeId = 8, Quantity = 9 },

                // Product 7 -> total 21
                new ProductSize { Id = 19, ProductId = 7, SizeId = 5, Quantity = 7 },
                new ProductSize { Id = 20, ProductId = 7, SizeId = 6, Quantity = 7 },
                new ProductSize { Id = 21, ProductId = 7, SizeId = 7, Quantity = 7 },

                // Product 8 -> total 30
                new ProductSize { Id = 22, ProductId = 8, SizeId = 6, Quantity = 10 },
                new ProductSize { Id = 23, ProductId = 8, SizeId = 7, Quantity = 10 },
                new ProductSize { Id = 24, ProductId = 8, SizeId = 8, Quantity = 10 },

                // Product 9 -> total 25
                new ProductSize { Id = 25, ProductId = 9, SizeId = 6, Quantity = 9 },
                new ProductSize { Id = 26, ProductId = 9, SizeId = 7, Quantity = 8 },
                new ProductSize { Id = 27, ProductId = 9, SizeId = 8, Quantity = 8 },

                // Product 10 -> total 30
                new ProductSize { Id = 28, ProductId = 10, SizeId = 1, Quantity = 10 },
                new ProductSize { Id = 29, ProductId = 10, SizeId = 2, Quantity = 10 },
                new ProductSize { Id = 30, ProductId = 10, SizeId = 3, Quantity = 10 },

                // Product 11 -> total 32
                new ProductSize { Id = 31, ProductId = 11, SizeId = 6, Quantity = 11 },
                new ProductSize { Id = 32, ProductId = 11, SizeId = 7, Quantity = 11 },
                new ProductSize { Id = 33, ProductId = 11, SizeId = 8, Quantity = 10 },

                // Product 12 -> total 30
                new ProductSize { Id = 34, ProductId = 12, SizeId = 1, Quantity = 10 },
                new ProductSize { Id = 35, ProductId = 12, SizeId = 2, Quantity = 10 },
                new ProductSize { Id = 36, ProductId = 12, SizeId = 3, Quantity = 10 },

                // Product 13 -> total 35
                new ProductSize { Id = 37, ProductId = 13, SizeId = 6, Quantity = 12 },
                new ProductSize { Id = 38, ProductId = 13, SizeId = 7, Quantity = 12 },
                new ProductSize { Id = 39, ProductId = 13, SizeId = 8, Quantity = 11 },

                // Product 14 -> total 28
                new ProductSize { Id = 40, ProductId = 14, SizeId = 1, Quantity = 10 },
                new ProductSize { Id = 41, ProductId = 14, SizeId = 2, Quantity = 9 },
                new ProductSize { Id = 42, ProductId = 14, SizeId = 3, Quantity = 9 },

                // Product 15 -> total 30
                new ProductSize { Id = 43, ProductId = 15, SizeId = 6, Quantity = 10 },
                new ProductSize { Id = 44, ProductId = 15, SizeId = 7, Quantity = 10 },
                new ProductSize { Id = 45, ProductId = 15, SizeId = 8, Quantity = 10 },

                // Product 16 -> total 35
                new ProductSize { Id = 46, ProductId = 16, SizeId = 6, Quantity = 12 },
                new ProductSize { Id = 47, ProductId = 16, SizeId = 7, Quantity = 12 },
                new ProductSize { Id = 48, ProductId = 16, SizeId = 8, Quantity = 11 },

                // Product 17 -> total 32
                new ProductSize { Id = 49, ProductId = 17, SizeId = 6, Quantity = 11 },
                new ProductSize { Id = 50, ProductId = 17, SizeId = 7, Quantity = 11 },
                new ProductSize { Id = 51, ProductId = 17, SizeId = 8, Quantity = 10 },

                // Product 18 -> total 33
                new ProductSize { Id = 52, ProductId = 18, SizeId = 6, Quantity = 11 },
                new ProductSize { Id = 53, ProductId = 18, SizeId = 7, Quantity = 11 },
                new ProductSize { Id = 54, ProductId = 18, SizeId = 8, Quantity = 11 },

                // Product 19 -> total 55
                new ProductSize { Id = 55, ProductId = 19, SizeId = 7, Quantity = 20 },
                new ProductSize { Id = 56, ProductId = 19, SizeId = 8, Quantity = 20 },
                new ProductSize { Id = 57, ProductId = 19, SizeId = 9, Quantity = 15 },

                // Product 20 -> total 60
                new ProductSize { Id = 58, ProductId = 20, SizeId = 1, Quantity = 20 },
                new ProductSize { Id = 59, ProductId = 20, SizeId = 2, Quantity = 20 },
                new ProductSize { Id = 60, ProductId = 20, SizeId = 3, Quantity = 20 }
            );

            // ProductSizes و Inventory (زي ما انت كاتب فوق) ✅
            // ... الكود اللي انت كاتبه للـ ProductSize و Inventory ينفع يتسيب زي ما هو 





            // Inventory لكل منتج (تاريخ ثابت بدل Dynamic)
            builder.Entity<Inventory>().HasData(
                new Inventory { Id = 1, ProductId = 1, QuantityAvailable = 24, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 2, ProductId = 2, QuantityAvailable = 26, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 3, ProductId = 3, QuantityAvailable = 33, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 4, ProductId = 4, QuantityAvailable = 18, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 5, ProductId = 5, QuantityAvailable = 25, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 6, ProductId = 6, QuantityAvailable = 28, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 7, ProductId = 7, QuantityAvailable = 21, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 8, ProductId = 8, QuantityAvailable = 30, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 9, ProductId = 9, QuantityAvailable = 25, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 10, ProductId = 10, QuantityAvailable = 30, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 11, ProductId = 11, QuantityAvailable = 32, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 12, ProductId = 12, QuantityAvailable = 30, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 13, ProductId = 13, QuantityAvailable = 35, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 14, ProductId = 14, QuantityAvailable = 28, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 15, ProductId = 15, QuantityAvailable = 30, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 16, ProductId = 16, QuantityAvailable = 35, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 17, ProductId = 17, QuantityAvailable = 32, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 18, ProductId = 18, QuantityAvailable = 33, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 19, ProductId = 19, QuantityAvailable = 55, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) },
                new Inventory { Id = 20, ProductId = 20, QuantityAvailable = 60, QuantityReserved = 0, SafetyStockThreshold = 5, LastStockChangeAt = new DateTime(2025, 1, 1) }
            );
        }
    }
}
