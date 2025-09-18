using FinalGraduationProject.Data;
using FinalGraduationProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();

        // تأكد من وجود الأدوار
        string adminRoleName = "Admin";
        string userRoleName = "User";

        if (await roleManager.FindByNameAsync(adminRoleName) == null)
        {
            await roleManager.CreateAsync(new IdentityRole<long>(adminRoleName));
        }

        if (await roleManager.FindByNameAsync(userRoleName) == null)
        {
            await roleManager.CreateAsync(new IdentityRole<long>(userRoleName));
        }

        // إنشاء مستخدم Admin
        var adminUser = new ApplicationUser
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            EmailConfirmed = true
        };

        var userExists = await userManager.FindByEmailAsync(adminUser.Email);
        if (userExists == null)
        {
            var result = await userManager.CreateAsync(adminUser, "Admin123*");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
        }
    }
}