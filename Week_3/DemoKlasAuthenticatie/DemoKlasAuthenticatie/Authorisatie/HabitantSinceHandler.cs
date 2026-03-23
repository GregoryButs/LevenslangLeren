using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DemoKlasAuthenticatie.Authorisatie
{
    public class HabitantSinceHandler : AuthorizationHandler<NationalityRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NationalityRequirement requirement)
        {
            Claim claim = context.User.FindFirst("HabitantSince");
            if (claim == null)
            {
                // Er is geen claim "HabitantSince", dus we kunnen de vereiste niet vervullen.
                return Task.CompletedTask;
            }
            if (DateTime.TryParse(claim.Value, out DateTime habitantSince))
            {
                if (habitantSince < DateTime.Now.AddYears(-5))
                {
                    // De gebruiker is al meer dan 5 jaar inwoner, dus we vervullen de vereiste.
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
            // De gebruiker voldoet niet aan de vereiste, dus we doen niets.
            return Task.CompletedTask;
        }
    }
}
