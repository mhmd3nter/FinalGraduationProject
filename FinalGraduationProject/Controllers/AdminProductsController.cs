using FinalGraduationProject.Data;
using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class AdminProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: AdminProducts
    [Authorize(Roles = "Admin")]

    public async Task<IActionResult> Dashboard()
    {
        // Website statistics
        ViewBag.UserCount = await _context.Users.CountAsync();
        ViewBag.OrderCount = await _context.Orders.CountAsync();
        ViewBag.ProductCount = await _context.Products.CountAsync();
        ViewBag.TotalSales = await _context.Orders.SumAsync(o => o.TotalAmount);

        // Recent orders (last 5)
        var recentOrders = await _context.Orders
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .Select(o => new
            {
                o.Id,
                UserEmail = _context.Users.Where(u => u.Id == o.UserId).Select(u => u.Email).FirstOrDefault(),
                o.OrderDate,
                o.Status,
                Total = o.TotalAmount
            })
            .ToListAsync();

        ViewBag.RecentOrders = recentOrders;

        return View();
    }

    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.Products.Include(p => p.Brand).Include(p => p.Category);
        return View(await applicationDbContext.ToListAsync());
        
        
    }

    // GET: AdminProducts/Create
    public IActionResult Create()
    {
        ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
    {
        Console.WriteLine($"BrandId: {product.BrandId}, CategoryId: {product.CategoryId}");

        if (ModelState.IsValid)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = "/images/" + fileName;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ✅ لو في أخطاء نحطها في ViewBag
        ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();

        ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
        return View(product);

        if (ModelState.IsValid)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Product saved successfully!");
            return RedirectToAction(nameof(Index));
        }
        else
        {
            Console.WriteLine("❌ ModelState not valid");
        }

    }
    // Index example (عشان ترجع للصفحة الرئيسية بعد Create)




    // GET: AdminProducts/Edit/5
    public async Task<IActionResult> Edit(long? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    // POST: AdminProducts/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, [Bind("Id,Name,Description,Price,Gender,Color,ImageUrl,IsActive,BrandId,CategoryId")] Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    // GET: AdminProducts/Delete/5
    public async Task<IActionResult> Delete(long? id)
    {
        if (id == null)
        {
            return NotFound();
        }

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

    // POST: AdminProducts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: AdminProducts/Details/5
    public async Task<IActionResult> Details(long? id)
    {
        if (id == null)
        {
            return NotFound();
        }

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

    private bool ProductExists(long id)
    {
        return _context.Products.Any(e => e.Id == id);
    }





}