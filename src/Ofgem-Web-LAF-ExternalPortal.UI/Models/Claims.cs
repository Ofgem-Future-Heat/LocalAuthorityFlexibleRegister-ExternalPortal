using Ofgem.LAF.SharedLibrary.Models;
using System.Security.Claims;
using System.Text.Json;

namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    public class Claims
    {

        public string EmailAddress { get; private set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public Ofgem.LAF.SharedLibrary.Enums.ExternalUserType ExternalUserType { get; private set; }

        public List<ExternalUserLocalAuthority>? LaList { get; private set; }
        public ExternalUserLocalAuthority? BaseLocalAuthority { get; private set; }

        public Claims(HttpContext httpContext)
        {
            if (httpContext.User.Identity is ClaimsIdentity claimsIdentity) BuildData(claimsIdentity);
        }

        public Claims(ClaimsPrincipal contextUser)
        {
            if (contextUser.Identity is ClaimsIdentity claimsIdentity) BuildData(claimsIdentity);
        }

        private void BuildData(ClaimsIdentity claimsIdentity)
        {
            var las = claimsIdentity.FindFirst("UserLocalAuthorities");

            if (las is null || las.Value is null) return;

            LaList = JsonSerializer.Deserialize<List<ExternalUserLocalAuthority>>(las.Value);

            if (LaList != null)
                foreach (var localAuthority in LaList)
                {
                    if (
                        localAuthority.IsBaseLocalAuthority != null &&
                        (bool)localAuthority.IsBaseLocalAuthority
                    )
                        BaseLocalAuthority = localAuthority;
                }

            var emailClaim = claimsIdentity.FindFirst(ClaimTypes.Email);

            if (emailClaim is not null) EmailAddress = emailClaim.Value;

            var firstNameClaim = claimsIdentity.FindFirst("FirstName");

            if (firstNameClaim is not null) FirstName = firstNameClaim.Value;

            var lastNameClaim = claimsIdentity.FindFirst("LastName");

            if (lastNameClaim is not null) LastName = lastNameClaim.Value;

            var userTypeClaim = claimsIdentity.FindFirst("UserType");

            if (userTypeClaim is not null)
            {
                ExternalUserType = Enum.TryParse(userTypeClaim.Value, out Ofgem.LAF.SharedLibrary.Enums.ExternalUserType externalUserType)
                    ? externalUserType
                    : Ofgem.LAF.SharedLibrary.Enums.ExternalUserType.LocalAuthorityOfficer;
            }
        }

        public string[] AsArrayOfStrings()
        {
            List<string> result = [];

            if (LaList == null) return [];

            result.AddRange(LaList.Select(x => x.OnsCode).OfType<string>());

            return [.. result];
        }
    }
}
