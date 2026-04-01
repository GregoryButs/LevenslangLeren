using Microsoft.AspNetCore.Authorization;
using Movie_Store.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MovieStore_StartHier_OK.Authorisation
{
    public class MinAgeHandler : AuthorizationHandler<MinAgeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinAgeRequirement requirement)
        {
            // Fix: Use 'is' pattern matching correctly and assign 'movie' only if the cast succeeds.
            if (!(context.Resource is Movie_Store.Models.Movie movie) || movie.AgeRestriction == null)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            Claim claim = context.User.FindFirst(c => c.Type == ClaimTypes.DateOfBirth);
            if (claim == null)
            {
                return Task.CompletedTask;
            }

            DateTime dateOfBirth = DateTime.Parse(claim.Value);

            int requiredAge = movie.AgeRestriction switch
            {
                AgeRestriction.Plus12 => 12,
                AgeRestriction.Plus16 => 16,
                AgeRestriction.Plus18 => 18,
                _ => 0
            };

            if (dateOfBirth.AddYears(requiredAge) <= DateTime.Now)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;

        }
    }
}

