using Microsoft.AspNetCore.Authorization;

namespace DemoKlasAuthenticatie.Authorisatie
{
    public class MinAgeRequirement: IAuthorizationRequirement
    {
        public int MinimumAge { get; }
        public MinAgeRequirement(int minimumAge)
        {
            MinimumAge = minimumAge;
        }
    }
}
