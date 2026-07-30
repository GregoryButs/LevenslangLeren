using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace AfsprakenbeheerPsycholoog.Extensions
{
    /// <summary>
    /// Extensies voor het efficiënt bepalen van gebruikersrollen en claims.
    /// </summary>
    public static class ClaimsExtensions
    {
        public static bool IsPsycholoog(this ClaimsPrincipal principal, IEnumerable<Claim>? userClaims = null)
        {
            if (principal?.Identity?.IsAuthenticated == true && principal.HasClaim("IsPsycholoog", "true"))
            {
                return true;
            }

            if (userClaims != null && userClaims.Any(c => c.Type == "IsPsycholoog" && c.Value == "true"))
            {
                return true;
            }

            return false;
        }
    }
}
