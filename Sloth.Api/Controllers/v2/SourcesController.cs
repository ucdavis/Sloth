using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sloth.Api.Attributes;
using Sloth.Api.Errors;
using Sloth.Core;
using Sloth.Api.Models.v2;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;

namespace Sloth.Api.Controllers.v2
{
    [Authorize(Policy = "ApiKey")]
    [VersionedRoute("2", "[controller]")]
    [ApiExplorerSettings(GroupName = "v2")]
    [ProducesResponseType(typeof(InternalExceptionResponse), 500)]
    public class SourcesController : Controller
    {
        private readonly SlothDbContext _context;
        public SourcesController(SlothDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<SourceModel>), 200)]
        public async Task<IList<SourceModel>> Get()
        {
            var teamId = GetTeamId();

            var sources = await _context.Sources
                .Where(s => s.Team.Id == teamId)
                .Select(SourceModel.Projection())
                .ToListAsync();

            return sources;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SourceModel), 200)]
        public async Task<SourceModel> Get(string id)
        {
            var teamId = GetTeamId();

            var source = await _context.Sources
                .Where(s => s.Name == id && s.Team.Id == teamId)
                .Select(SourceModel.Projection())
                .FirstOrDefaultAsync();

            return source;
        }

        private string GetTeamId()
        {
            return User.FindFirst(ClaimTypes.PrimaryGroupSid)?.Value;
        }

    }
}
