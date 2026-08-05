using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Extensions;
using AfsprakenbeheerPsycholoog.Profiles;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using System.IO.Compression;

namespace AfsprakenbeheerPsycholoog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Initialiseer statische merk- en afbeeldingassets
            AssetInitializerExtensions.InitializeAssets();

            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=/app/data/Afsprakenbeheer.db";
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            var keysFolder = Path.Combine(AppContext.BaseDirectory, "data", "keys");
            if (!Directory.Exists(keysFolder)) Directory.CreateDirectory(keysFolder);
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

            // Web API Controllers instead of MVC Views
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = string.Join(" ", context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { message = $"We konden de afspraak niet opslaan door een validatiefout: {errors}" });
                    };
                });

            // Caching & Performance Services
            builder.Services.AddMemoryCache();
            builder.Services.AddResponseCaching();
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });
            builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            // Configure CORS for React frontend
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("PsycholoogOnly",
                    policy => policy.RequireClaim("IsPsycholoog", "true"));
                options.AddPolicy("HasPatientProfile",
                    policy => policy.RequireClaim("PatientId"));
            });

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                // Account
                options.SignIn.RequireConfirmedAccount = true;

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
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

            // Configure External OAuth Logins (Google, Microsoft, Facebook, Apple)
            var authBuilder = builder.Services.AddAuthentication();

            var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
            var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            if (!string.IsNullOrEmpty(googleClientId) && googleClientId != "YOUR_GOOGLE_CLIENT_ID")
            {
                authBuilder.AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret ?? "";
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                });
            }

            var msClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
            var msClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
            if (!string.IsNullOrEmpty(msClientId) && msClientId != "YOUR_MICROSOFT_CLIENT_ID")
            {
                authBuilder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = msClientId;
                    options.ClientSecret = msClientSecret ?? "";
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                });
            }

            var fbAppId = builder.Configuration["Authentication:Facebook:AppId"];
            var fbAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
            if (!string.IsNullOrEmpty(fbAppId) && fbAppId != "YOUR_FACEBOOK_APP_ID")
            {
                authBuilder.AddFacebook(options =>
                {
                    options.AppId = fbAppId;
                    options.AppSecret = fbAppSecret ?? "";
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                });
            }

            var appleClientId = builder.Configuration["Authentication:Apple:ClientId"];
            if (!string.IsNullOrEmpty(appleClientId) && appleClientId != "YOUR_APPLE_CLIENT_ID")
            {
                authBuilder.AddOpenIdConnect("Apple", options =>
                {
                    options.Authority = "https://appleid.apple.com";
                    options.ClientId = appleClientId;
                    options.CallbackPath = "/signin-apple";
                    options.ResponseType = "code id_token";
                    options.ResponseMode = "form_post";
                    options.Scope.Add("name");
                    options.Scope.Add("email");
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                });
            }

            // Configure cookie behaviors for API (return 401/403 instead of redirecting to login page)
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
            });

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
            builder.Services.AddScoped<IPatientBookingService, PatientBookingService>();
            builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddScoped<IAIService, AIService>();

            // Configure Rate Limiting to prevent spam bookings and brute-force auth attempts
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter("booking-policy", opt =>
                {
                    opt.PermitLimit = 5; // Maximaal 5 afspraken per periode
                    opt.Window = TimeSpan.FromMinutes(10); // Venster van 10 minuten
                    opt.QueueLimit = 0;
                    opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                });
                options.AddFixedWindowLimiter("auth-policy", opt =>
                {
                    opt.PermitLimit = 20; // Maximaal 20 inlog/registratie pogingen per minuut
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                    opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                });
            });

            // Background Service voor automatische acties op afspraken
            builder.Services.AddHostedService<AfspraakStatusUpdaterService>();

            // Background worker & Queue voor asynchrone Google Calendar en e-mail synchronisatie
            builder.Services.AddSingleton<CalendarSyncQueue>();
            builder.Services.AddHostedService<CalendarSyncBackgroundWorker>();

            // Background service voor het versturen van geplande herinneringen
            builder.Services.AddHostedService<ReminderBackgroundService>();

            var app = builder.Build();

            // Forwarded Headers instellen voor Reverse Proxy / Docker (HTTPS borgen voor OAuth redirect URI's)
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
            };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseHsts();
            }

            // Seeding en migratie van de database
            app.SeedAndMigrateDatabase();

            // app.UseHttpsRedirection();

            // Custom HTTP Security Headers Middleware
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
                await next();
            });

            app.UseResponseCompression();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseResponseCaching();

            // Enable rate limiting
            app.UseRateLimiter();

            // Use CORS before authentication and authorization
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            // Map API controllers
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}

