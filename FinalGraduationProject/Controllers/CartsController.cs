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

        public CartsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: لعرض محتويات سلة المستخدم
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
                                         .ThenInclude(p => p.Inventory)  // جلب بيانات المخزون مع المنتج
                                         .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                userCart = new Cart { UserId = userId };
                _context.Carts.Add(userCart);
                await _context.SaveChangesAsync();
            }

            return View(userCart.CartItems);
        }

        // POST: لإضافة منتج إلى السلة
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

            var userCart = await _context.Carts.Include(c => c.CartItems)
                                               .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                userCart = new Cart { UserId = userId };
                _context.Carts.Add(userCart);
            }

            var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
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
                userCart.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: لحذف منتج من السلة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(long cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: لتحديث كمية منتج في السلة (زيادة أو نقصان)
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

            // التحقق من عدم تجاوز المخزون المتوفر
            if (cartItem.Product.Inventory != null && newQuantity > cartItem.Product.Inventory.QuantityAvailable)
            {
                ModelState.AddModelError("", "الكمية المطلوبة أكبر من المخزون المتوفر.");
                // ممكن تعيد الكارت مع رسالة الخطأ مثلا
                return RedirectToAction(nameof(Index));
            }

            if (newQuantity < 1)
            {
                // لو الكمية أقل من 1 نحذف العنصر من الكارت
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
    }
}
