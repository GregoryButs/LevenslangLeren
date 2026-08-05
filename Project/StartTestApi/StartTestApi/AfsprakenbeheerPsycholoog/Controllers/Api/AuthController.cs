using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Extensions;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IPatientRepository patientRepo,
            IServiceScopeFactory scopeFactory,
            ILogger<AuthController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _patientRepo = patientRepo;
            _scopeFactory = scopeFactory;
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
                bool isPsycholoog = User.IsPsycholoog(claims);
                bool isProfileComplete = isPsycholoog || user.Geboortedatum.HasValue;

                return Ok(new
                {
                    Email = user.Email,
                    Voornaam = user.Voornaam,
                    Achternaam = user.Achternaam,
                    IsPsycholoog = isPsycholoog,
                    PatientId = user.PatientId,
                    Geboortedatum = user.Geboortedatum?.ToString("yyyy-MM-dd"),
                    IsProfileComplete = isProfileComplete
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

            var matchingPatient = _patientRepo.GetAll()
                .FirstOrDefault(p => p.IsActief && !string.IsNullOrEmpty(p.Email) && p.Email.Trim().ToLower() == model.Email.Trim().ToLower());

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Voornaam = model.Voornaam,
                Achternaam = model.Achternaam,
                PatientId = matchingPatient?.Id,
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
                        using var scope = _scopeFactory.CreateScope();
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.EmailConfirmed)
            {
                // Ter beveiliging geen indicatie geven of het account wel of niet bestaat
                return Ok(new { message = "Indien het e-mailadres bij ons bekend is, heeft u een e-mail ontvangen met instructies om uw wachtwoord te herstellen." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = $"{Request.Scheme}://{Request.Host}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

            var subject = "Wachtwoord herstellen - De Verstandhouding";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #f1f5f9; border-radius: 12px;'>
                    <h2 style='color: #1a2c30;'>Wachtwoord Herstellen</h2>
                    <p style='color: #475569;'>Beste {user.Voornaam},</p>
                    <p style='color: #475569;'>Er is een verzoek ingediend om het wachtwoord van uw account bij <strong>De Verstandhouding</strong> te herstellen.</p>
                    <p style='color: #475569;'>Klik op de onderstaande knop om een nieuw wachtwoord in te stellen:</p>
                    <div style='margin: 30px 0; text-align: center;'>
                        <a href='{resetUrl}' style='background-color: #478d96; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 50px; font-weight: bold; display: inline-block;'>Nieuw Wachtwoord Instellen</a>
                    </div>
                    <p style='color: #94a3b8; font-size: 12px;'>Als u dit verzoek niet heeft ingediend, kunt u deze e-mail negeren.</p>
                    <p style='color: #94a3b8; font-size: 12px;'>Met vriendelijke groet,<br>Praktijk De Verstandhouding</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email, subject, body);
            return Ok(new { message = "Indien het e-mailadres bij ons bekend is, heeft u een e-mail ontvangen met instructies om uw wachtwoord te herstellen." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Wachtwoord herstellen mislukt. Controleer uw gegevens." });
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { message = "Uw wachtwoord is succesvol gewijzigd. U kunt nu inloggen met uw nieuwe wachtwoord." });
            }

            var firstError = result.Errors.FirstOrDefault()?.Description ?? "Wachtwoord herstellen mislukt.";
            return BadRequest(new { message = firstError });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Succesvol uitgelogd." });
        }

        [HttpGet("external-login")]
        public async Task<IActionResult> ExternalLogin(
            [FromQuery] string provider, 
            [FromQuery] string returnUrl, 
            [FromServices] IAuthenticationSchemeProvider schemeProvider,
            [FromServices] IWebHostEnvironment env)
        {
            if (string.IsNullOrEmpty(provider))
            {
                return BadRequest(new { message = "Provider is verplicht." });
            }

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = $"{Request.Scheme}://{Request.Host}/external-auth-callback";
            }

            var scheme = await schemeProvider.GetSchemeAsync(provider);
            if (scheme == null)
            {
                _logger.LogWarning("Poging tot external login voor provider '{Provider}', maar deze scheme is niet geregistreerd in Authentication.", provider);
                
                // In Development mode, als credentials nog niet zijn ingesteld, geef een heldere melding
                var errorMsg = $"De inlogprovider '{provider}' is nog niet geconfigureerd in UserSecrets of AppSettings op de server. Stel de ClientId en ClientSecret in.";
                return Redirect($"{returnUrl}?status=failed&message={Uri.EscapeDataString(errorMsg)}");
            }

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("external-login-callback")]
        [HttpPost("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback([FromQuery] string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = $"{Request.Scheme}://{Request.Host}/external-auth-callback";
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("GetExternalLoginInfoAsync retourneerde null voor de callback op {ReturnUrl}.", returnUrl);
                return Redirect($"{returnUrl}?status=failed&message=Gegevens+van+externe+provider+konden+niet+worden+opgehaald.");
            }

            // 1. Probeer in te loggen indien dit OAuth account reeds is gekoppeld
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("Gebruiker succesvol ingelogd via externe provider '{Provider}'.", info.LoginProvider);
                return Redirect($"{returnUrl}?status=success");
            }

            // 2. Haal het e-mailadres op uit de claims (of fallback naar Apple name/email claims)
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Geen e-mailadres gevonden in claims voor provider '{Provider}'.", info.LoginProvider);
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

                var matchingPatient = _patientRepo.GetAll()
                    .FirstOrDefault(p => p.IsActief && !string.IsNullOrEmpty(p.Email) && p.Email.Trim().ToLower() == email.Trim().ToLower());

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Voornaam = voornaam,
                    Achternaam = achternaam,
                    EmailConfirmed = true, // Social logins (Google/Microsoft/Facebook/Apple) hebben een geverifieerd e-mailadres
                    PatientId = matchingPatient?.Id
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var firstError = createResult.Errors.FirstOrDefault()?.Description ?? "Fout bij aanmaken gebruiker.";
                    _logger.LogError("Fout bij aanmaken ApplicationUser via external login: {Error}", firstError);
                    return Redirect($"{returnUrl}?status=failed&message={Uri.EscapeDataString(firstError)}");
                }
            }
            else
            {
                bool userUpdated = false;
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    userUpdated = true;
                }

                if (!user.PatientId.HasValue)
                {
                    var matchingPatient = _patientRepo.GetAll()
                        .FirstOrDefault(p => p.IsActief && !string.IsNullOrEmpty(p.Email) && p.Email.Trim().ToLower() == email.Trim().ToLower());
                    if (matchingPatient != null)
                    {
                        user.PatientId = matchingPatient.Id;
                        userUpdated = true;
                        _logger.LogInformation("Bestaande gebruiker '{Email}' automatisch gekoppeld aan patient ID {PatientId}.", user.Email, matchingPatient.Id);
                    }
                }

                if (userUpdated)
                {
                    await _userManager.UpdateAsync(user);
                }
            }

            // 4. Koppel de externe login aan de ApplicationUser
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (addLoginResult.Succeeded || addLoginResult.Errors.Any(e => e.Code == "UserAlreadyHasLogin"))
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("Externe login '{Provider}' succesvol gekoppeld aan gebruiker '{Email}'.", info.LoginProvider, user.Email);
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
            bool isPsycholoog = User.IsPsycholoog(claims);
            bool isProfileComplete = isPsycholoog || user.Geboortedatum.HasValue;

            return Ok(new
            {
                Email = user.Email,
                Voornaam = user.Voornaam,
                Achternaam = user.Achternaam,
                IsPsycholoog = isPsycholoog,
                PatientId = user.PatientId,
                Geboortedatum = user.Geboortedatum?.ToString("yyyy-MM-dd"),
                IsProfileComplete = isProfileComplete
            });
        }

        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDto model)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            if (!DateOnly.TryParse(model.Geboortedatum, out var geboortedatum))
            {
                return BadRequest(new { message = "Ongeldige geboortedatum opgegeven." });
            }

            user.Geboortedatum = geboortedatum;
            await _userManager.UpdateAsync(user);

            // Als de gebruiker reeds gekoppeld is aan een patiënt, overschrijf dan de geboortedatum van de patiënt!
            if (user.PatientId.HasValue)
            {
                var patient = _patientRepo.GetById(user.PatientId.Value);
                if (patient != null)
                {
                    patient.Geboortedatum = geboortedatum;
                    if (!string.IsNullOrWhiteSpace(model.Telefoonnummer))
                    {
                        patient.Telefoonnummer = model.Telefoonnummer.Trim();
                    }
                    _patientRepo.Update(patient);
                    _patientRepo.SaveChanges();
                }
            }

            var claims = await _userManager.GetClaimsAsync(user);
            bool isPsycholoog = User.IsPsycholoog(claims);

            return Ok(new
            {
                Email = user.Email,
                Voornaam = user.Voornaam,
                Achternaam = user.Achternaam,
                IsPsycholoog = isPsycholoog,
                PatientId = user.PatientId,
                Geboortedatum = user.Geboortedatum?.ToString("yyyy-MM-dd"),
                IsProfileComplete = true,
                message = "Profiel succesvol bijgewerkt."
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

    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Token { get; set; } = null!;

        [Required]
        [StringLength(100, ErrorMessage = "Wachtwoord moet minimaal {2} tekens bevatten.", MinimumLength = 6)]
        public string NewPassword { get; set; } = null!;
    }

    public class CompleteProfileDto
    {
        [Required(ErrorMessage = "Geboortedatum is verplicht.")]
        public string Geboortedatum { get; set; } = null!;

        public string? Telefoonnummer { get; set; }
    }
}
