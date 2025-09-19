using FinalGraduationProject.Data; // مجلد البيانات
using FinalGraduationProject.Models; // مجلد الموديل
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IList<Product> Products { get; set; } = default!;


        public async Task<IActionResult> Index()
        {
            Products = await _context.Products
    .Include(p => p.Brand)
    .Include(p => p.Category)
    .Include(p => p.ProductSizes)
        .ThenInclude(ps => ps.Size)
    .ToListAsync();

            // استرجاع كل المنتجات من قاعدة البيانات
            return View(Products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductSizes)
                    .ThenInclude(ps => ps.Size)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }



        

    }
}