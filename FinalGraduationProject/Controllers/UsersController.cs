using FinalGraduationProject.Data;
using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<long>> _roleManager;
    private readonly ApplicationDbContext _context;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<long>> roleManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    // List all users
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        return View(users);
    }

    // Make user Admin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeAdmin(long id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        if (!await _roleManager.RoleExistsAsync("Admin"))
            await _roleManager.CreateAsync(new IdentityRole<long>("Admin"));

        await _userManager.AddToRoleAsync(user, "Admin");
        TempData["SuccessMessage"] = "User promoted to Admin.";
        return RedirectToAction("Index");
    }

    // Remove Admin role (make just a user)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAdmin(long id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        await _userManager.RemoveFromRoleAsync(user, "Admin");

        // If the current user is being demoted, sign them out
        if (user.Id == long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            TempData["ErrorMessage"] = "You have been removed from the admin role and logged out.";
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        TempData["SuccessMessage"] = "User demoted to regular user.";
        return RedirectToAction("Index");
    }

    // Block user (disable login)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(long id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        await _userManager.UpdateAsync(user);
        TempData["SuccessMessage"] = "User blocked.";
        return RedirectToAction("Index");
    }

    // Unblock user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(long id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        user.LockoutEnd = null;
        await _userManager.UpdateAsync(user);
        TempData["SuccessMessage"] = "User unblocked.";
        return RedirectToAction("Index");
    }

    // Delete user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        await _userManager.DeleteAsync(user);
        TempData["SuccessMessage"] = "User deleted.";
        return RedirectToAction("Index");
    }
}