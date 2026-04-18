using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Profiles;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AfsprakenbeheerPsycholoog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("PsycholoogOnly",
                    policy => policy.RequireClaim("IsPsycholoog", "true"));
            });

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                // Account
                options.SignIn.RequireConfirmedAccount = false;

                // Lockout (brute force protection)
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                // Password
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // AutoMapper
            builder.Services.AddAutoMapper(
                           _ => { },
                           (typeof(AfspraakProfile).Assembly),
                           (typeof(PatientProfile).Assembly));

            // Repositories
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<IAfspraakRepository, AfspraakRepository>();
            builder.Services.AddScoped<IAfspraakTypeRepository, AfspraakTypeRepository>();

            // Services
            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<IAfspraakService, AfspraakService>();
            builder.Services.AddScoped<IAfspraakTypeService, AfspraakTypeService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IPatientBoekService, PatientBoekService>();

            // Background Service voor automatische acties op afspraken
            builder.Services.AddHostedService<AfspraakStatusUpdaterService>();

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // seeding data toevoegen aan de database
            using (var scope = app.Services.CreateScope())
            {
                SeedData.Initialize(scope.ServiceProvider).GetAwaiter().GetResult();
            }



            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
