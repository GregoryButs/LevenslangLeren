using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Extensions;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AfsprakenbeheerPsycholoog.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth-policy")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IPatientRepository _patientRepo;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IPatientRepository patientRepo,
            IServiceProvider serviceProvider,
            ILogger<AuthController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _patientRepo = patientRepo;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Ongeldig e-mailadres of wachtwoord." });
            }

            if (user.PatientId.HasValue)
            {
                var patient = _patientRepo.GetById(user.PatientId.Value);
                if (patient != null && !patient.IsActief)
                {
                    return BadRequest(new { message = "Dit account is gedeactiveerd. Neem contact op met de praktijk." });
                }
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(new { 
                    message = "Gelieve eerst uw e-mailadres te bevestigen via de link in de e-mail die we u gestuurd hebben.",
                    requireEmailConfirmation = true,
                    email = user.Email 
                });
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var claims = await _userManager.GetClaimsAsync(user);

                return Ok(new
                {
                    Email = user.Email,
                    Voornaam = user.Voornaam,
                    Achternaam = user.Achternaam,
                    IsPsycholoog = User.IsPsycholoog(claims),
                    PatientId = user.PatientId
                });
            }

            if (result.IsNotAllowed)
            {
                return BadRequest(new { 
                    message = "Gelieve eerst uw e-mailadres te bevestigen via de link in de e-mail die we u gestuurd hebben.",
                    requireEmailConfirmation = true,
                    email = user.Email
                });
            }

            if (result.IsLockedOut)
            {
                return StatusCode(StatusCodes.Status423Locked, new { message = "Account is tijdelijk vergrendeld vanwege te veel mislukte inlogpogingen." });
            }

            return BadRequest(new { message = "Ongeldig e-mailadres of wachtwoord." });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                bool isUserInactive = false;

                if (existingUser.PatientId.HasValue)
                {
                    var patient = _patientRepo.GetById(existingUser.PatientId.Value);
                    if (patient == null || !patient.IsActief)
                    {
                        isUserInactive = true;
                    }
                }
                else
                {
                    // Geen actieve patiënt gekoppeld aan het account
                    isUserInactive = true;
                }

                if (isUserInactive)
                {
                    // Oude gedeactiveerde gebruiker verwijderen zodat de registratie opnieuw kan worden uitgevoerd
                    await _userManager.DeleteAsync(existingUser);
                }
                else
                {
                    return BadRequest(new { message = "Er bestaat al een actief account met dit e-mailadres." });
                }
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Voornaam = model.Voornaam,
                Achternaam = model.Achternaam,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Generate Email Confirmation Token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmUrl = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                var subject = "Bevestig uw e-mailadres - De Verstandhouding";
                var body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #f1f5f9; border-radius: 12px;'>
                        <h2 style='color: #1a2c30;'>Welkom bij De Verstandhouding</h2>
                        <p style='color: #475569;'>Beste {user.Voornaam},</p>
                        <p style='color: #475569;'>Bedankt voor uw registratie bij praktijk <strong>De Verstandhouding</strong>.</p>
                        <p style='color: #475569;'>Gelieve op de onderstaande knop te klikken om uw e-mailadres te bevestigen en uw account te activeren:</p>
                        <div style='margin: 30px 0; text-align: center;'>
                            <a href='{confirmUrl}' style='background-color: #478d96; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 50px; font-weight: bold; display: inline-block;'>E-mailadres Bevestigen</a>
                        </div>
                        <p style='color: #94a3b8; font-size: 12px;'>Indien de knop niet werkt, kunt u de volgende link kopiëren in uw browser:<br><a href='{confirmUrl}' style='color: #478d96;'>{confirmUrl}</a></p>
                        <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                        <p style='color: #94a3b8; font-size: 12px;'>Met vriendelijke groet,<br>Praktijk De Verstandhouding</p>
                    </div>";

                var userEmail = user.Email;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        await emailService.SendEmailAsync(userEmail, subject, body);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Fout bij achtergrond-verzending van registratie-email naar {Email}: {Details}", userEmail, ex.ToString());
                    }
                });

                return Ok(new
                {
                    requireEmailConfirmation = true,
                    message = "Registratie gelukt! Er is een bevestigingsmail naar uw e-mailadres gestuurd. Klik op de link in de mail om in te loggen."
                });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Ongeldige bevestigingslink.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Gebruiker niet gevonden.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Content(@"
                    <!DOCTYPE html>
                    <html lang='nl'>
                    <head>
                        <title>E-mailadres Bevestigd - De Verstandhouding</title>
                        <meta charset='utf-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <style>
                            body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f2f9f9; color: #1a2c30; display: flex; align-items: center; justify-content: center; min-height: 100vh; margin: 0; padding: 20px; }
                            .card { background: white; padding: 40px; border-radius: 24px; box-shadow: 0 10px 25px rgba(0,0,0,0.05); text-align: center; max-width: 450px; width: 100%; }
                            .icon { width: 64px; height: 64px; background: #e0f2f1; color: #478d96; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 20px auto; font-size: 32px; }
                            h2 { color: #1a2c30; margin-bottom: 12px; }
                            p { color: #475569; font-size: 15px; line-height: 1.5; margin-bottom: 24px; }
                            .btn { display: inline-block; padding: 14px 32px; background-color: #478d96; color: white; border-radius: 50px; text-decoration: none; font-weight: bold; font-size: 15px; }
                        </style>
                    </head>
                    <body>
                        <div class='card'>
                            <div class='icon'>✓</div>
                            <h2>E-mailadres Bevestigd!</h2>
                            <p>Uw e-mailadres is succesvol geverifieerd. Uw account is nu geactiveerd en u bent ingelogd.</p>
                            <a href='/portal' class='btn'>Ga naar het Patiëntenportaal</a>
                        </div>
                    </body>
                    </html>", "text/html");
            }

            return BadRequest("E-mailbevestiging is mislukt of de verificatielink is reeds gebruikt/verlopen.");
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email)) return BadRequest(new { message = "E-mailadres is verplicht." });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.EmailConfirmed)
            {
                return Ok(new { message = "Indien het account bestaat en nog niet geverifieerd is, is er een nieuwe bevestigingsmail verzonden." });
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmUrl = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var subject = "Bevestig uw e-mailadres - De Verstandhouding";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #f1f5f9; border-radius: 12px;'>
                    <h2 style='color: #1a2c30;'>Herhaalde Bevestigingsmail</h2>
                    <p style='color: #475569;'>Beste {user.Voornaam},</p>
                    <p style='color: #475569;'>Klik op de onderstaande knop om uw e-mailadres te bevestigen en uw account bij <strong>De Verstandhouding</strong> te activeren:</p>
                    <div style='margin: 30px 0; text-align: center;'>
                        <a href='{confirmUrl}' style='background-color: #478d96; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 50px; font-weight: bold; display: inline-block;'>E-mailadres Bevestigen</a>
                    </div>
                    <p style='color: #94a3b8; font-size: 12px;'>Met vriendelijke groet,<br>Praktijk De Verstandhouding</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email, subject, body);
            return Ok(new { message = "Een nieuwe bevestigingsmail is verzonden naar uw e-mailadres." });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Succesvol uitgelogd." });
        }

        [HttpGet("external-login")]
        public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string returnUrl)
        {
            if (string.IsNullOrEmpty(provider))
            {
                return BadRequest(new { message = "Provider is verplicht." });
            }

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback([FromQuery] string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "http://localhost:5173/external-auth-callback";
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Redirect($"{returnUrl}?status=failed&message=Gegevens+van+externe+provider+konden+niet+worden+opgehaald.");
            }

            // 1. Probeer in te loggen indien dit OAuth account reeds is gekoppeld
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return Redirect($"{returnUrl}?status=success");
            }

            // 2. Haal het e-mailadres op uit de claims
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return Redirect($"{returnUrl}?status=failed&message=E-mailadres+is+niet+vrijgegeven+door+de+inlogprovider.");
            }

            // 3. Zoek of de gebruiker al bestaat met dit e-mailadres
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var voornaam = info.Principal.FindFirstValue(ClaimTypes.GivenName) 
                    ?? info.Principal.FindFirstValue(ClaimTypes.Name)?.Split(' ').FirstOrDefault() 
                    ?? "Gebruiker";
                
                var achternaam = info.Principal.FindFirstValue(ClaimTypes.Surname) 
                    ?? info.Principal.FindFirstValue(ClaimTypes.Name)?.Split(' ').LastOrDefault() 
                    ?? "";

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Voornaam = voornaam,
                    Achternaam = achternaam,
                    EmailConfirmed = true, // Social logins (Google/Microsoft/Apple) have verified emails
                    PatientId = null
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var firstError = createResult.Errors.FirstOrDefault()?.Description ?? "Fout bij aanmaken gebruiker.";
                    return Redirect($"{returnUrl}?status=failed&message={Uri.EscapeDataString(firstError)}");
                }
            }
            else if (!user.EmailConfirmed)
            {
                // Auto-confirm if logging in via verified external provider
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            // 4. Koppel de externe login aan de ApplicationUser
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (addLoginResult.Succeeded || addLoginResult.Errors.Any(e => e.Code == "UserAlreadyHasLogin"))
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect($"{returnUrl}?status=success");
            }

            return Redirect($"{returnUrl}?status=failed&message=Koppeling+van+externe+login+is+mislukt.");
        }

        [DisableRateLimiting]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var claims = await _userManager.GetClaimsAsync(user);

            return Ok(new
            {
                Email = user.Email,
                Voornaam = user.Voornaam,
                Achternaam = user.Achternaam,
                IsPsycholoog = User.IsPsycholoog(claims),
                PatientId = user.PatientId
            });
        }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }

    public class RegisterDto
    {
        [Required]
        public string Voornaam { get; set; } = null!;

        [Required]
        public string Achternaam { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(100, ErrorMessage = "Wachtwoord moet minimaal {2} tekens bevatten.", MinimumLength = 6)]
        public string Password { get; set; } = null!;
    }

    public class ResendConfirmationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
