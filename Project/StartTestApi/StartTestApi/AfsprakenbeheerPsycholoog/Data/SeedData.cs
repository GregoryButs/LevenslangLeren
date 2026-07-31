using AfsprakenbeheerPsycholoog.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AfsprakenbeheerPsycholoog.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var adminEmail = configuration?["AdminUser:Email"] 
                ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") 
                ?? "inge@deverstandhouding.be";

            var adminPassword = configuration?["AdminUser:Password"] 
                ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD") 
                ?? "Admin123!";

            // Migreer of zoek oud admin account
            var oldAdmin = await userManager.FindByEmailAsync("ingedebast@deverstandhouding.be");
            if (oldAdmin != null && adminEmail == "inge@deverstandhouding.be")
            {
                oldAdmin.Email = adminEmail;
                oldAdmin.UserName = adminEmail;
                oldAdmin.EmailConfirmed = true;
                await userManager.UpdateAsync(oldAdmin);
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Voornaam = "Inge",
                    Achternaam = "Debast",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, adminPassword);
                await userManager.AddClaimAsync(admin,
                    new System.Security.Claims.Claim("IsPsycholoog", "true"));
            }
            else
            {
                // Garandeer dat admin altijd het IsPsycholoog claim heeft
                var claims = await userManager.GetClaimsAsync(admin);
                if (!claims.Any(c => c.Type == "IsPsycholoog" && c.Value == "true"))
                {
                    await userManager.AddClaimAsync(admin, new System.Security.Claims.Claim("IsPsycholoog", "true"));
                }
            }
        }
    }
}
