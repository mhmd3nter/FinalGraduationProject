using FinalGraduationProject.Data;
using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FinalGraduationProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this._context = context;
        }
        public IList<Product> Products { get; set; } = default!;
        public async Task<IActionResult> IndexAsync()
        {
            Products = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.ProductSizes)
            .ThenInclude(ps => ps.Size)
            .ToListAsync();

            return View(Products);
        }

        public async Task<IActionResult> Search(string? searchTerm)
        {
            var result = new SearchResultVM();
            try
            {
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    result.Products = await _context.Products
                        .Include(p => p.Brand)
                        .Include(p => p.Category)
                        .Include(p => p.ProductSizes)
                            .ThenInclude(ps => ps.Size)
                        .Where(p => p.Name != null && p.Name.Contains(searchTerm))
                        .ToListAsync();


                    if (!result.Products.Any())
                    {
                        ViewData["SearchMessage"] = $"No results found for '{searchTerm}'.";
                    }
                }
                else
                {
                    ViewData["SearchMessage"] = "Please enter a search term.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during search.");
                ViewData["SearchMessage"] = "An error occurred while searching. Please try again later.";
            }
            return View(result);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
