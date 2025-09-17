using FinalGraduationProject.Data;
using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinalGraduationProject.Controllers
{
    [Authorize]
    public class ShipmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShipmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: صفحة اختيار الشحن
        public async Task<IActionResult> Create(long orderId)
        {
            var order = await _context.Orders
                                      .Include(o => o.OrderItems)
                                      .ThenInclude(oi => oi.Product)
                                      .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            ViewBag.ShippingMethods = await _context.ShippingMethods.ToListAsync();

            return View(order);
        }

        // POST: تأكيد عملية الشحن
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(long orderId, long shippingMethodId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            var shipment = new Shipment
            {
                OrderId = orderId,
                ShippingMethodId = shippingMethodId,
                TrackingNumber = Guid.NewGuid().ToString().Substring(0, 10).ToUpper(),
                ShippedAt = DateTime.Now
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Orders", new { id = orderId });
        }
    }
}
