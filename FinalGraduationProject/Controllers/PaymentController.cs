using FinalGraduationProject.Data;
using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FinalGraduationProject.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Show payment options
        public async Task<IActionResult> Index(long orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Brand) // Include brand for display
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Index", "Home");
            }

            // Check if order is already paid
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == "Completed");

            if (existingPayment != null)
            {
                TempData["InfoMessage"] = "This order has already been paid.";
                return RedirectToAction("Success", new { orderId = orderId });
            }

            ViewBag.OrderId = orderId;
            return View(order);
        }

        // POST: Process payment with Paymob
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(long orderId, string paymentMethod)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Inventory)
                .FirstOrDefaultAsync(o => o.Id == orderId);
                
            if (order == null)
                return NotFound();

            try
            {
                // Simple payment processing
                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = order.TotalAmount,
                    Method = paymentMethod,
                    Status = "Pending"
                };

                if (paymentMethod == "Cash")
                {
                    // Cash on delivery - mark as confirmed
                    payment.Status = "Completed";
                    order.Status = "Confirmed";
                }
                else if (paymentMethod == "Paymob")
                {
                    // For now, we'll simulate Paymob payment
                    // In real implementation, you would integrate with Paymob API
                    payment.Status = "Completed";
                    order.Status = "Paid";
                    
                    TempData["SuccessMessage"] = "Payment processed successfully!";
                }

                _context.Payments.Add(payment);

                // ?? Update inventory stock after successful payment
                foreach (var orderItem in order.OrderItems)
                {
                    if (orderItem.Product?.Inventory != null)
                    {
                        var inventory = orderItem.Product.Inventory;
                        inventory.QuantityAvailable -= orderItem.Quantity;
                        inventory.LastStockChangeAt = DateTime.UtcNow;
                        
                        // Make sure stock doesn't go negative
                        if (inventory.QuantityAvailable < 0)
                        {
                            inventory.QuantityAvailable = 0;
                        }
                    }
                }

                // ?? Empty the user's cart after successful payment
                var userCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == order.UserId);

                if (userCart != null && userCart.CartItems.Any())
                {
                    _context.CartItems.RemoveRange(userCart.CartItems);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Success", new { orderId = orderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Payment failed. Please try again.";
                return RedirectToAction("Index", new { orderId = orderId });
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
    }
}