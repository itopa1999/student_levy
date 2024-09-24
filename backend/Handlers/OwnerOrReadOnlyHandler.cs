using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace backend.Handlers
{
    public class OwnerOrReadOnlyHandler : AuthorizationHandler<OwnerOrReadOnlyRequirement>
    {
         private readonly IHttpContextAccessor _httpContextAccessor;

        public OwnerOrReadOnlyHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnerOrReadOnlyRequirement requirement)
        {
            // Assume the resource ID is passed in the route as {id}
            var resourceOwnerId = _httpContextAccessor.HttpContext?.Request.RouteValues["id"];
            var userId = context.User.FindFirst(c => c.Type == "sub")?.Value; // Assuming "sub" is the user ID claim

            // Check if the user is the owner
            if (resourceOwnerId != null && resourceOwnerId.ToString() == userId)
            {
                context.Succeed(requirement); // User is the owner
                return Task.CompletedTask;
            }

            // If not the owner, allow read-only access
            if (context.User.IsInRole("Reader")) // Assuming "Reader" is a role for read-only access
            {
                context.Succeed(requirement); // User has read-only access
            }

            return Task.CompletedTask;
        }
    }
}