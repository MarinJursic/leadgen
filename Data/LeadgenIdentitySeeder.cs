using Leadgen.Model.Entities;
using leadgen.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace leadgen.Data;

public static class LeadgenIdentitySeeder
{
    public const string AdminEmail = "admin@leadgen.local";
    public const string AdminPassword = "LeadgenAdmin1!";
    public const string ManagerEmail = "manager@leadgen.local";
    public const string ManagerPassword = "LeadgenManager1!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        foreach (var role in new[] { LeadgenRoles.Admin, LeadgenRoles.Manager })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var admin = await EnsureUserAsync(
            userManager,
            AdminEmail,
            AdminPassword,
            "Leadgen Admin",
            "12345678901",
            "1234567890123");
        await EnsureRoleAsync(userManager, admin, LeadgenRoles.Admin);
        await EnsureRoleAsync(userManager, admin, LeadgenRoles.Manager);

        var manager = await EnsureUserAsync(
            userManager,
            ManagerEmail,
            ManagerPassword,
            "Leadgen Manager",
            "23456789012",
            "2345678901234");
        await EnsureRoleAsync(userManager, manager, LeadgenRoles.Manager);
    }

    private static async Task<AppUser> EnsureUserAsync(
        UserManager<AppUser> userManager,
        string email,
        string password,
        string displayName,
        string oib,
        string jmbg)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            return user;
        }

        user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName,
            OIB = oib,
            JMBG = jmbg
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not seed user {email}: {errors}");
        }

        return user;
    }

    private static async Task EnsureRoleAsync(UserManager<AppUser> userManager, AppUser user, string role)
    {
        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
