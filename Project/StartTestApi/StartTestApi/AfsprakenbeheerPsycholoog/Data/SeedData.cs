using AfsprakenbeheerPsycholoog.Authentication;
using Microsoft.AspNetCore.Identity;

namespace AfsprakenbeheerPsycholoog.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var adminEmail = "ingedebast@deverstandhouding.be";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Voornaam = "Inge",
                    Achternaam = "Debast",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin123");
                await userManager.AddClaimAsync(admin,
                    new System.Security.Claims.Claim("IsPsycholoog", "true"));
            }
        }
    }
}


