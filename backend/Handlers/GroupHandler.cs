using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace backend.Handlers
{
    public class GroupHandler : AuthorizationHandler<GroupRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupRequirement requirement)
        {
            // Check if the user has the "group" claim
            var groupClaim = context.User.FindFirst(c => c.Type == "group");

            if (groupClaim != null && requirement.GroupNames.Contains(groupClaim.Value))
            {
                context.Succeed(requirement);  // Success if the user is in one of the groups
            }

            return Task.CompletedTask;
        }
    }
}