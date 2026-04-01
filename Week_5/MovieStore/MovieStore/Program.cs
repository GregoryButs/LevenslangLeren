using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Movie_Store.Services;
using MovieStore_StartHier_OK.Authorisation;
using MovieStore_StartHier_OK.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// geef mee dat je sessions wilt gebruiken
builder.Services.AddSession();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// nieuw claims policy toevoegen aan de user op basis van claim ProductManagerOnly
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ProductManagerOnly", policy => policy.RequireClaim("CanEditMovies"));
});

builder.Services.AddSingleton<IAuthorizationHandler, MinAgeHandler>();


builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("IsAllowedToBuy", policy => policy.AddRequirements(new MinAgeRequirement()));
            });

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// geef mee dat je sessions wilt gebruiken
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movie}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
