using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sloth.Web.Controllers
{
    [Authorize]
    public class SuperController : Controller
    {
    }
}
