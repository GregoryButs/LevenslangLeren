using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DemoKlasAuthenticatie.Authorisatie
{
    public class MinAgeHandler : AuthorizationHandler<MinAgeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinAgeRequirement requirement)
        {
            Claim claim = context.User.FindFirst(c => c.Type == ClaimTypes.DateOfBirth);
            if (claim == null)
            {
                return Task.CompletedTask;
            }

            DateTime dateOfBirth = Convert.ToDateTime(claim.Value);
            DateTime cutOff = DateTime.Now.AddYears(-requirement.MinimumAge);
            int age = DateTime.Now.Year - dateOfBirth.Year;
            if (dateOfBirth > cutOff)
            {
                age--;
            }
            if (age >= requirement.MinimumAge)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
    }
}
