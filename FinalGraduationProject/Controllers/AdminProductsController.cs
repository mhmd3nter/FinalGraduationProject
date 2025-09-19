using FinalGraduationProject.Data;
using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FinalGraduationProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminProducts
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            // Website statistics
            ViewBag.UserCount = await _context.Users.CountAsync();
            ViewBag.OrderCount = await _context.Orders
                .Where(o => o.Status != "Cancelled" && o.Status!= "Pending")
                .CountAsync();
            ViewBag.ProductCount = await _context.Products.CountAsync();
            ViewBag.TotalSales = await _context.Orders
                .Where(o => o.Status != "Cancelled" && o.Status != "Pending")
                .SumAsync(o => o.TotalAmount);

            // Recent orders (last 5)
            var recentOrders = await _context.Orders
                .Where(o => o.Status != "Pending")
                .Include(o => o.User) // ⬅️ يجيب بيانات اليوزر مباشرة
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    o.Id,
                    UserEmail = o.User.Email,
                    o.OrderDate,
                    o.Status,
                    Total = o.TotalAmount
                })
                .ToListAsync();
            ViewBag.RecentOrders = recentOrders;

            return View();
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.Brand).Include(p => p.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AdminProducts/Create
        public async Task<IActionResult> Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Sizes = await _context.Sizes.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile, Dictionary<int, SizeQuantityDto> SizeQuantities)
        {
            // Debug logging
            Console.WriteLine($"BrandId: {product.BrandId}, CategoryId: {product.CategoryId}");
            Console.WriteLine($"Name: {product.Name}, Gender: {product.Gender}, Color: {product.Color}");
            Console.WriteLine($"Price: {product.Price}");

            // Log size quantities
            if (SizeQuantities != null)
            {
                Console.WriteLine("Size Quantities:");
                foreach (var sq in SizeQuantities)
                {
                    Console.WriteLine($"Size {sq.Key}: Selected={sq.Value.IsSelected}, Quantity={sq.Value.Quantity}");
                }
            }

            // Log ModelState errors
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Errors:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"{state.Key}: {error.ErrorMessage}");
                    }
                }
            }

            // Remove ProductSizes from validation since we handle it manually
            ModelState.Remove("ProductSizes");
            ModelState.Remove("Brand");
            ModelState.Remove("Category");
            ModelState.Remove("Inventory");
            ModelState.Remove("OrderItems");
            ModelState.Remove("CartItems");

            // Remove SizeQuantities from validation
            var keysToRemove = ModelState.Keys.Where(k => k.StartsWith("SizeQuantities")).ToList();
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    // Create images directory if it doesn't exist
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images"));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    product.ImageUrl = "/images/" + fileName;
                }

                // Add the product first
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Create ProductSizes for selected sizes
                if (SizeQuantities != null && SizeQuantities.Any())
                {
                    var productSizes = new List<ProductSize>();
                    var totalQuantity = 0;

                    foreach (var sizeQuantity in SizeQuantities.Values)
                    {
                        if (sizeQuantity.IsSelected && sizeQuantity.Quantity > 0)
                        {
                            productSizes.Add(new ProductSize
                            {
                                ProductId = product.Id,
                                SizeId = sizeQuantity.SizeId,
                                Quantity = sizeQuantity.Quantity
                            });
                            totalQuantity += sizeQuantity.Quantity;
                            Console.WriteLine($"Added ProductSize: SizeId={sizeQuantity.SizeId}, Quantity={sizeQuantity.Quantity}");
                        }
                    }

                    if (productSizes.Any())
                    {
                        _context.ProductSizes.AddRange(productSizes);

                        // Create inventory record
                        var inventory = new Inventory
                        {
                            ProductId = product.Id,
                            QuantityAvailable = totalQuantity,
                            QuantityReserved = 0,
                            SafetyStockThreshold = 5,
                            LastStockChangeAt = DateTime.UtcNow
                        };
                        _context.Inventories.Add(inventory);

                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ Created {productSizes.Count} ProductSizes with total quantity: {totalQuantity}");
                    }
                }

                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // If we got here, something failed, redisplay form
            ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Sizes = await _context.Sizes.ToListAsync();
            return View(product);
        }

        // GET: AdminProducts/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: AdminProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,Description,Price,Gender,Color,ImageUrl,IsActive,BrandId,CategoryId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: AdminProducts/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: AdminProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: AdminProducts/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        private bool ProductExists(long id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}