using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RecallApp_API.Authorization
{
    public class UserOwnerHandler : AuthorizationHandler<UserOwnerRequirement, Guid>
    {
        protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserOwnerRequirement requirement,
        Guid userID)
        {

            // Ownership check
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userId, out Guid authenticatedUsertId) &&
                authenticatedUsertId == userID)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
