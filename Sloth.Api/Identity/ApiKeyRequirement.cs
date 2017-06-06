using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Sloth.Api.Identity
{
    public class ApiKeyRequirement : IAuthorizationRequirement
    {
        public ApiKeyRequirement()
        {
        }
    }
}
