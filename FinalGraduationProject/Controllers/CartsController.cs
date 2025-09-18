using FinalGraduationProject.Data;
using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinalGraduationProject.Controllers
{
    [Authorize]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cart Contents
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
            {
                return RedirectToAction("Index", "Home");
            }

            var userCart = await _context.Carts
                                         .Include(c => c.CartItems)
                                         .ThenInclude(ci => ci.Product)
                                         .ThenInclude(p => p.Brand) // Include Brand for display
                                         .Include(c => c.CartItems)
                                         .ThenInclude(ci => ci.Product)
                                         .ThenInclude(p => p.Inventory) // 🔧 FIX: Include Inventory
                                         .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                userCart = new Cart { UserId = userId };
                _context.Carts.Add(userCart);
                await _context.SaveChangesAsync();
                
                // Return empty cart items since we just created the cart
                return View(new List<CartItem>());
            }

            return View(userCart.CartItems);
        }

        // ✅ POST: Add product to cart with TempData & redirect back to Products
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddToCart(long productId, int quantity)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
            {
                return RedirectToAction("Index", "Home");
            }

            // Check if product exists and has stock
            var product = await _context.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction("Index", "Products");
            }

            // Check stock availability
            var availableStock = product.Inventory?.QuantityAvailable ?? 0;
            if (availableStock < quantity)
            {
                TempData["ErrorMessage"] = "Not enough stock available.";
                return RedirectToAction("Index", "Products");
            }

            var userCart = await _context.Carts
                                         .Include(c => c.CartItems)
                                         .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                userCart = new Cart { UserId = userId };
                _context.Carts.Add(userCart);
                await _context.SaveChangesAsync(); // Save to get the cart ID
            }

            var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                // Check if adding more quantity exceeds stock
                if (cartItem.Quantity + quantity > availableStock)
                {
                    TempData["ErrorMessage"] = "Cannot add more items. Not enough stock.";
                    return RedirectToAction("Index", "Products");
                }
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    CartId = userCart.Id
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // ✅ Add success message to TempData
            TempData["CartMessage"] = "✅ Product added to cart!";

            // ✅ Redirect back to products list
            return RedirectToAction("Index", "Products");
        }

        // POST: Remove from cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(long cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
                
            if (cartItem != null)
            {
                var productName = cartItem.Product?.Name ?? "Item";
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"✅ {productName} removed from cart!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Update quantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(long cartItemId, int change)
        {
            var cartItem = await _context.CartItems
                                         .Include(ci => ci.Product)
                                         .ThenInclude(p => p.Inventory)
                                         .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }

            int newQuantity = cartItem.Quantity + change;

            if (cartItem.Product.Inventory != null && newQuantity > cartItem.Product.Inventory.QuantityAvailable)
            {
                ModelState.AddModelError("", "الكمية المطلوبة أكبر من المخزون المتوفر.");
                return RedirectToAction(nameof(Index));
            }

            if (newQuantity < 1)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = newQuantity;
                _context.CartItems.Update(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Update quantity directly (from input field)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantityDirect(long cartItemId, int newQuantity)
        {
            var cartItem = await _context.CartItems
                                         .Include(ci => ci.Product)
                                         .ThenInclude(p => p.Inventory)
                                         .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }

            // Validate quantity
            if (newQuantity < 1)
            {
                TempData["ErrorMessage"] = "Quantity must be at least 1.";
                return RedirectToAction(nameof(Index));
            }

            var availableStock = cartItem.Product?.Inventory?.QuantityAvailable ?? 0;
            if (newQuantity > availableStock)
            {
                TempData["ErrorMessage"] = $"Only {availableStock} items available in stock.";
                return RedirectToAction(nameof(Index));
            }

            // Update quantity silently (no success message)
            cartItem.Quantity = newQuantity;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            // No success message - just redirect back to cart
            return RedirectToAction(nameof(Index));
        }

        // Helper method to calculate order totals
        private static decimal CalculateSubtotal(IEnumerable<CartItem> cartItems)
        {
            return cartItems.Where(item => item.Product != null)
                           .Sum(item => item.Product.Price * item.Quantity);
        }

        private static decimal CalculateTax(decimal subtotal)
        {
            return subtotal * 0.14m; // 14% tax
        }

        private static decimal CalculateShipping(decimal subtotal)
        {
            return subtotal > 100 ? 0 : 10; // Free shipping over $100
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
            {
                return RedirectToAction("Index", "Home");
            }

            // Get cart with products
            var userCart = await _context.Carts
                                         .Include(c => c.CartItems)
                                         .ThenInclude(ci => ci.Product)
                                         .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null || !userCart.CartItems.Any())
                return RedirectToAction(nameof(Index));

            // Create the order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                OrderItems = userCart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product?.Price ?? 0m
                }).ToList()
            };

            // Calculate total amount using helper methods
            var subtotal = CalculateSubtotal(userCart.CartItems);
            var tax = CalculateTax(subtotal);
            var shipping = CalculateShipping(subtotal);
            order.TotalAmount = subtotal + tax + shipping;

            // Add order to database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Redirect to payment page
            return RedirectToAction("Index", "Payment", new { orderId = order.Id });
        }

    }

}

