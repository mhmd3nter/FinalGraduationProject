using FinalGraduationProject.Data;
using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinalGraduationProject.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Show payment options
        public async Task<IActionResult> Index(long orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Brand)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Index", "Home");
            }

            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == "Completed");

            if (existingPayment != null)
            {
                TempData["InfoMessage"] = "This order has already been paid.";
                return RedirectToAction("Success", new { orderId });
            }

            ViewBag.OrderId = orderId;
            return View(order);
        }

        // POST: Process payment (with or without new address)
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(long orderId, string paymentMethod, Address? address)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Inventory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            // ?? ?? ??????? ????? ????? ????? ????? ??? ?? ?????? ? ??? ???????? ???? ?????
            if (!order.AddressId.HasValue && address == null)
            {
                TempData["ErrorMessage"] = "Please enter a shipping address before continuing.";
                return RedirectToAction("AddAddress", new { orderId, paymentMethod });
            }

            // ? ?? ???? ????? ?? ??????? ??? ?? Address ??? ?? ?????? ? ????
            if (!order.AddressId.HasValue && address != null)
            {
                address.UserId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();

                order.AddressId = address.Id;
                await _context.SaveChangesAsync();
            }

            try
            {
                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = order.TotalAmount,
                    Method = paymentMethod,
                    Status = "Pending"
                };

                if (paymentMethod == "Cash")
                {
                    payment.Status = "Completed";
                    order.Status = "Confirmed";
                }
                else if (paymentMethod == "Paymob")
                {
                    payment.Status = "Completed";
                    order.Status = "Paid";
                    TempData["SuccessMessage"] = "Payment processed successfully!";
                }

                _context.Payments.Add(payment);

                // ????? ???????
                foreach (var orderItem in order.OrderItems)
                {
                    if (orderItem.Product?.Inventory != null)
                    {
                        var inventory = orderItem.Product.Inventory;
                        inventory.QuantityAvailable -= orderItem.Quantity;
                        inventory.LastStockChangeAt = DateTime.UtcNow;
                        if (inventory.QuantityAvailable < 0)
                            inventory.QuantityAvailable = 0;
                    }
                }

                // ????? ??????
                var userCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == order.UserId);

                if (userCart != null && userCart.CartItems.Any())
                    _context.CartItems.RemoveRange(userCart.CartItems);

                await _context.SaveChangesAsync();

                return RedirectToAction("Success", new { orderId });
            }
            catch
            {
                TempData["ErrorMessage"] = "Payment failed. Please try again.";
                return RedirectToAction("Index", new { orderId });
            }
        }

        // GET: Payment success page
        public async Task<IActionResult> Success(long orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: Add new address page
        public IActionResult AddAddress(long orderId, string paymentMethod)
        {
            ViewBag.OrderId = orderId;
            ViewBag.PaymentMethod = paymentMethod;
            return View(new Address());
        }
    }
}
