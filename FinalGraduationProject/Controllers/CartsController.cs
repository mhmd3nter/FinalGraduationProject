using FinalGraduationProject.Data;
using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinalGraduationProject.Controllers
{
    [Authorize]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cart Contents
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductSize)
                        .ThenInclude(ps => ps.Product)
                            .ThenInclude(p => p.Brand)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductSize)
                        .ThenInclude(ps => ps.Size)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                userCart = new Cart { UserId = userId };
                _context.Carts.Add(userCart);
                await _context.SaveChangesAsync();
                return View(new List<CartItem>());
            }

            return View(userCart.CartItems);
        }

        // POST: Add product size to cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(long productId, int quantity, long productSizeId, string color)
        {
            var user = await _userManager.GetUserAsync(User);
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Forbid(); // or return a view/message indicating not allowed
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var productSize = await _context.ProductSizes
                .Include(ps => ps.Product)
                .Include(ps => ps.Size)
                .FirstOrDefaultAsync(ps => ps.Id == productSizeId);

            if (productSize == null)
            {
                TempData["ErrorMessage"] = "❌ Product size not found.";
                return RedirectToAction("Index", "Products");
            }

            if (productSize.Quantity < quantity)
            {
                TempData["ErrorMessage"] = $"❌ Only {productSize.Quantity} items available.";
                return RedirectToAction("Index", "Products");
            }

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                userCart = new Cart { UserId = userId };
                _context.Carts.Add(userCart);
                await _context.SaveChangesAsync();
            }

            var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.ProductSizeId == productSizeId);

            if (cartItem != null)
            {
                if (cartItem.Quantity + quantity > productSize.Quantity)
                {
                    TempData["ErrorMessage"] = "❌ Not enough stock for this size.";
                    return RedirectToAction("Index", "Products");
                }
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = userCart.Id,
                    ProductSizeId = productSizeId,
                    ProductId = productSize.ProductId, // <-- Add this line
                    Quantity = quantity
                };
                userCart.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            TempData["CartMessage"] = $"✅ Added {quantity} × {productSize.Product.Name} (Size {productSize.Size.Name})";
            return RedirectToAction("Index", "Products");
        }

        // POST: Remove item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(long cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.ProductSize)
                    .ThenInclude(ps => ps.Product)
                .Include(ci => ci.ProductSize.Size)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem != null)
            {
                var productName = cartItem.ProductSize.Product.Name;
                var sizeName = cartItem.ProductSize.Size.Name;
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ {productName} (Size {sizeName}) removed from cart.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Update quantity (+/- buttons)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(long cartItemId, int change)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.ProductSize)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null) return NotFound();

            var newQuantity = cartItem.Quantity + change;

            if (newQuantity < 1)
            {
                _context.CartItems.Remove(cartItem);
            }
            else if (newQuantity > cartItem.ProductSize.Quantity)
            {
                TempData["ErrorMessage"] = $"❌ Only {cartItem.ProductSize.Quantity} items available.";
            }
            else
            {
                cartItem.Quantity = newQuantity;
                _context.CartItems.Update(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Update quantity directly (input field)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantityDirect(long cartItemId, int newQuantity)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.ProductSize)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null) return NotFound();

            if (newQuantity < 1)
            {
                TempData["ErrorMessage"] = "❌ Quantity must be at least 1.";
                return RedirectToAction(nameof(Index));
            }

            if (newQuantity > cartItem.ProductSize.Quantity)
            {
                TempData["ErrorMessage"] = $"❌ Only {cartItem.ProductSize.Quantity} items available.";
                return RedirectToAction(nameof(Index));
            }

            cartItem.Quantity = newQuantity;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===== Helpers for totals =====
        private static decimal CalculateSubtotal(IEnumerable<CartItem> cartItems)
        {
            return cartItems.Sum(item => (item.ProductSize.Product.Price) * item.Quantity);
        }

        private static decimal CalculateTax(decimal subtotal) => subtotal * 0.14m;
        private static decimal CalculateShipping(decimal subtotal) => subtotal > 100 ? 0 : 10;

        // POST: Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null || !userCart.CartItems.Any())
                return RedirectToAction(nameof(Index));

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                OrderItems = userCart.CartItems.Select(ci => new OrderItem
                {
                    ProductSizeId = ci.ProductSizeId,
                    ProductId = ci.ProductSize.ProductId, // <-- Fix: set ProductId
                    Quantity = ci.Quantity,
                    Price = ci.ProductSize.Product.Price
                }).ToList()
            };

            var subtotal = CalculateSubtotal(userCart.CartItems);
            var tax = CalculateTax(subtotal);
            var shipping = CalculateShipping(subtotal);
            order.TotalAmount = subtotal + tax + shipping;

            _context.Orders.Add(order);

            // ✅ Update stock quantities
            foreach (var ci in userCart.CartItems)
            {
                ci.ProductSize.Quantity -= ci.Quantity;
                _context.ProductSizes.Update(ci.ProductSize); // Ensure EF tracks the change
            }

            _context.CartItems.RemoveRange(userCart.CartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Payment", new { orderId = order.Id });
        }
    }
}
