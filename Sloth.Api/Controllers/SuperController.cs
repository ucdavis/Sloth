using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sloth.Api.Attributes;
using Sloth.Api.Errors;

namespace Sloth.Api.Controllers
{
    [Authorize(Policy = "ApiKey")]
    [VersionedRoute("[controller]")]
    [ProducesResponseType(typeof(InternalExceptionResponse), 500)]
    public class SuperController : Controller
    {
    }
}
