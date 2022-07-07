using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sloth.Api.Attributes;
using Sloth.Api.Errors;

namespace Sloth.Api.Controllers.v1
{
    [Authorize(Policy = "ApiKey")]
    [VersionedRoute("[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ProducesResponseType(typeof(InternalExceptionResponse), 500)]
    public class SuperController : Controller
    {
    }
}
