using FinalGraduationProject.Data;
using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Antiforgery;
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
        private readonly IAntiforgery _antiforgery;


        public CartsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,IAntiforgery antiforgery)
        {
            _context = context;
            _userManager = userManager;
            _antiforgery = antiforgery;
        }
        // 🟢 AJAX AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartAjax([FromBody] AddToCartDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return Json(new { success = false, message = "❌ User not found" });

            var productSize = await _context.ProductSizes
                .Include(ps => ps.Product)
                .Include(ps => ps.Size)
                .FirstOrDefaultAsync(ps => ps.Id == dto.ProductSizeId);

            if (productSize == null)
                return Json(new { success = false, message = "❌ Product size not found." });

            if (productSize.Quantity < dto.Quantity)
                return Json(new { success = false, message = $"❌ Only {productSize.Quantity} items available." });

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                userCart = new Cart { UserId = userId };
                _context.Carts.Add(userCart);
                await _context.SaveChangesAsync();
            }

            var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.ProductSizeId == dto.ProductSizeId);

            if (cartItem != null)
            {
                if (cartItem.Quantity + dto.Quantity > productSize.Quantity)
                    return Json(new { success = false, message = "❌ Not enough stock for this size." });

                cartItem.Quantity += dto.Quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = userCart.Id,
                    ProductSizeId = dto.ProductSizeId,
                    ProductId = productSize.ProductId,
                    Quantity = dto.Quantity
                };
                userCart.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            var cartCount = await _context.CartItems
                .Where(ci => ci.CartId == userCart.Id)
                .SumAsync(ci => ci.Quantity);

            return Json(new { success = true, message = "✅ Added successfully", cartCount });
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

            // Load all ProductSizes for each product in the cart (for size selection)
            var productIds = userCart.CartItems.Select(ci => ci.ProductId).Distinct().ToList();
            var productSizesDict = await _context.ProductSizes
                .Include(ps => ps.Size)
                .Where(ps => productIds.Contains(ps.ProductId))
                .GroupBy(ps => ps.ProductId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            foreach (var item in userCart.CartItems)
            {
                if (item.Product != null && productSizesDict.TryGetValue(item.ProductId, out var sizes))
                {
                    item.Product.ProductSizes = sizes;
                }
            }

            return View(userCart.CartItems);
        }

        // POST: Add product size to cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(long productId, int quantity, long productSizeId, string color)
        {
            if (!User.Identity.IsAuthenticated)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType == "application/json")
                    return Unauthorized(new { success = false, message = "Please log in first to add items to your cart." });
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return Forbid();

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

        // POST: Update size
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSize(long cartItemId, long productSizeId)
        {
            var productSize = await _context.ProductSizes.FindAsync(productSizeId);
            if (productSize == null)
            {
                TempData["ErrorMessage"] = "Selected size does not exist.";
                return RedirectToAction("Index");
            }

            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                cartItem.ProductSizeId = productSizeId;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
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
