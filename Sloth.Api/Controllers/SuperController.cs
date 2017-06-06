using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sloth.Api.Attributes;

namespace Sloth.Api.Controllers
{
    [Authorize(Policy = "ApiKey")]
    [VersionedRoute("[controller]")]
    public class SuperController : Controller
    {
    }
}
