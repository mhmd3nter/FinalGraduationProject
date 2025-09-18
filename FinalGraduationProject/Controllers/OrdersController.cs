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
                .Where(o => o.UserId == userId)
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
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Admin: View details of any order
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDetails(long id)
        {
            var order = await _context.Orders
                .Include(o => o.User) // <-- This line ensures User is loaded
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize.Size)
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

                return RedirectToAction("MyOrders");
            }
        }
    } 