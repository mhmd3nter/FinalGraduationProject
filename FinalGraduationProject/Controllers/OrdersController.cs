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
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // User: View their own orders
        public async Task<IActionResult> MyOrders()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize.Size)
                .Include(o => o.Address)
                .Where(o => o.UserId == userId) // <-- include cancelled orders as well
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // User: View details of a specific order
        public async Task<IActionResult> Details(long id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize.Size)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            return View(order);
        }




        // Admin: Manage all orders
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize.Size)
                .Include(o => o.Address) // <-- Include address
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Admin: View details of any order
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDetails(long id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize.Size)
                .Include(o => o.Address) // <-- Include address
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // Admin: Edit order status
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditStatus(long id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();
            return View(order);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(long id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            order.Status = status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("AdminDetails", new { id = order.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var userIdString = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            if (order.Status != "Confirmed")
            {
                TempData["ErrorMessage"] = "You can only delete orders with status 'Confirmed'.";
                return RedirectToAction("MyOrders");
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order deleted successfully.";
            return RedirectToAction("MyOrders");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string address, string phoneNumber /*, other parameters */)
        {
            var user = await _userManager.GetUserAsync(User);
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Forbid();
            }

            // Save address and phone number with the order
            // Example:
            // var order = new Order { ... };
            // order.Address = address;
            // order.PhoneNumber = phoneNumber;
            // _context.Orders.Add(order);
            // await _context.SaveChangesAsync();

            // After order and payment are successful:
            var userId = user.Id;
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                // Optionally: _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MyOrders");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(long id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            // Mark as cancelled, but do NOT set a reason
            order.Status = "Cancelled";
            order.CancellationReason = null; // Ensure it's null for user-initiated cancels
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order cancelled successfully.";
            return RedirectToAction("MyOrders");
        }

        // User: Edit their own order (only if the status is Pending)
        public async Task<IActionResult> Edit(long id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize.Size)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null || order.Status != "Pending")
                return NotFound();

            // Get all products and sizes for selection
            ViewBag.Products = await _context.Products.Include(p => p.ProductSizes).ThenInclude(ps => ps.Size).ToListAsync();

            // Fix: Cast ViewBag.Products to List<Product> before using LINQ
            var products = ViewBag.Products as List<Product>;
            ViewBag.ProductSizesJson = System.Text.Json.JsonSerializer.Serialize(
                products.Select(p => new
                {
                    productId = p.Id,
                    sizes = p.ProductSizes.Select(ps => new
                    {
                        id = ps.Id,
                        name = ps.Size.Name
                    })
                })
            );

            return View(order);
        }

        // User: Update their own order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Order updatedOrder, long[] addProductIds, long[] addSizeIds, int[] addQuantities)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null || order.Status != "Pending")
                return NotFound();

            // Work on a stable list of existing items and use indexes consistently
            var existingItems = order.OrderItems.ToList();
            for (int i = 0; i < existingItems.Count; i++)
            {
                var item = existingItems[i];
                var formPrefix = $"OrderItems[{i}]";
                var remove = Request.Form[$"{formPrefix}.Remove"].ToString();
                var newSizeId = Request.Form[$"{formPrefix}.ProductSizeId"].ToString();
                var newQuantity = Request.Form[$"{formPrefix}.Quantity"].ToString();

                // Checkbox may post "true" or "on" depending on client; handle both
                if (!string.IsNullOrEmpty(remove) && (remove.Equals("true", StringComparison.OrdinalIgnoreCase) || remove.Equals("on", StringComparison.OrdinalIgnoreCase)))
                {
                    _context.OrderItems.Remove(item);
                    continue;
                }

                if (long.TryParse(newSizeId, out var sizeId))
                    item.ProductSizeId = sizeId;
                if (int.TryParse(newQuantity, out var qty))
                    item.Quantity = qty;
            }

            // Ensure add arrays are not null to avoid NullReferenceException
            addProductIds ??= Array.Empty<long>();
            addSizeIds ??= Array.Empty<long>();
            addQuantities ??= Array.Empty<int>();

            // Add new items (ensure arrays lengths align)
            var addCount = Math.Min(addProductIds.Length, Math.Min(addSizeIds.Length, addQuantities.Length));
            for (int i = 0; i < addCount; i++)
            {
                if (addProductIds[i] > 0 && addSizeIds[i] > 0 && addQuantities[i] > 0)
                {
                    var ps = await _context.ProductSizes.FindAsync(addSizeIds[i]);
                    if (ps == null) continue; // skip invalid size

                    // Prefer product id from ps to avoid trusting client addProductIds
                    var productId = ps.ProductId;

                    var price = 0m;
                    // get price from navigation if loaded, otherwise load product price
                    if (ps.Product != null)
                        price = ps.Product.Price;
                    else
                    {
                        var product = await _context.Products.FindAsync(productId);
                        price = product?.Price ?? 0m;
                    }

                    var newItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = productId,            // <-- IMPORTANT: set ProductId
                        ProductSizeId = ps.Id,
                        Quantity = addQuantities[i],
                        Price = price
                    };
                    _context.OrderItems.Add(newItem);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = order.Id });
        }

        // ========================
        // Checkout & Payment
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(long orderId, string paymentMethod, string address)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("MyOrders");
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                TempData["ErrorMessage"] = "Please enter your address.";
                return RedirectToAction("Details", new { id = orderId });
            }

            // Save payment data
            order.Address = new Address
            {
                UserId = userId,
                Street = address, // You may want to split address into Street, City, etc.
                // Set other Address properties as needed, e.g. City, State, PostalCode, Country
            };

            order.Status = "AddressConfirmed";

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            // Redirect to confirmation page
            return RedirectToAction("ConfirmAddress", new { id = orderId });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmAddress(long id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("MyOrders");
            }

            return View(order); // ConfirmAddress.cshtml
        }

        // Admin: Cancel an order (soft delete, e.g. mark as cancelled)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminDelete(long id, string reason)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            order.Status = "Cancelled";
            order.CancellationReason = reason;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            // Optionally: Notify user via email or notification system

            TempData["SuccessMessage"] = $"Order #{order.Id} cancelled. User will be notified: {reason}";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCancelledOrders()
        {
            var userIdString = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long userId))
                return RedirectToAction("Index", "Home");

            var cancelledOrders = await _context.Orders
                .Where(o => o.UserId == userId && o.Status == "Cancelled")
                .ToListAsync();

            _context.Orders.RemoveRange(cancelledOrders);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cancelled orders cleared.";
            return RedirectToAction("MyOrders");
        }

    }
}