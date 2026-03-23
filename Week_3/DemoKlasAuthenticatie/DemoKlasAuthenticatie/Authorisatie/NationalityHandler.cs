using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DemoKlasAuthenticatie.Authorisatie
{
    public class NationalityHandler : AuthorizationHandler<NationalityRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NationalityRequirement requirement)
        {
            // in claim staat de nationaliteit van de gebruiker, deze kunnen we vergelijken met de vereiste nationaliteit in het requirement
            Claim claim = context.User.FindFirst("Nationality");

            // als er geen claim is, kunnen we niet voldoen aan de vereiste nationaliteit, dus kunnen we de handler beëindigen zonder het requirement te markeren als geslaagd
            if (claim == null)
            {
                return Task.CompletedTask;
            }
            string nationality = claim.Value;
            if (nationality == "Belgian")
            {
                // als de nationaliteit overeenkomt met de vereiste nationaliteit, markeren we het requirement als geslaagd
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            else
            {
                // als de nationaliteit niet overeenkomt, kunnen we de handler beëindigen zonder het requirement te markeren als geslaagd
                return Task.CompletedTask;
            }
        }
    }
}
