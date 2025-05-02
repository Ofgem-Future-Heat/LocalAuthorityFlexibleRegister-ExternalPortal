using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    public class MustHaveClaimLocalAuthoritiesRequirement(string claimType) : IAuthorizationRequirement
    {
        public string ClaimType { get; } = claimType;
    }

    public class MustHaveClaimHandler : AuthorizationHandler<MustHaveClaimLocalAuthoritiesRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustHaveClaimLocalAuthoritiesRequirement requirement)
        {
            var claim = context.User.Claims.FirstOrDefault(s => s.Type == requirement.ClaimType);

            if (claim is null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (claim.Value is null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var items = JsonSerializer.Deserialize<List<ExternalUserLocalAuthority>>(claim.Value);

            if (items is null)
            {
                context.Fail();
                return Task.CompletedTask;

            }

            if (items.Count == 0)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
