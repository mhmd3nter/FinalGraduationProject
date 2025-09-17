using FinalGraduationProject.Data; // مجلد البيانات
using FinalGraduationProject.Models; // مجلد الموديل
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // لعمليات قاعدة البيانات

namespace FinalGraduationProject.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context; // للوصول لقاعدة البيانات

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            // استرجاع كل المنتجات من قاعدة البيانات
            var products = await _context.Products.Include(p => p.Brand).Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // البحث عن منتج بالـ Id
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
    }
}