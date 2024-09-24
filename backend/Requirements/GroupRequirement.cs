using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace backend.Requirements
{
    public class GroupRequirement : IAuthorizationRequirement
    {
        public string GroupNames { get; }

        public GroupRequirement(string groupNames)
        {
            GroupNames = groupNames;
        }
    }
}