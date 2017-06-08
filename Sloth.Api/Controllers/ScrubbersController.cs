using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;

namespace Sloth.Api.Controllers
{
    public class ScrubbersController : SuperController
    {
        private readonly SlothDbContext _context;

        public ScrubbersController(SlothDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Fetch Top 1 Scrubber
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<Scrubber>), 200)]
        public async Task<IList<Scrubber>> Get()
        {
            var scrubbers = await _context.Scrubbers
                .Take(1)
                .ToListAsync();

            return scrubbers;
        }

        [HttpGet("{id}")]
        public async Task<Scrubber> Get(int id)
        {
            var scrubber = await _context.Scrubbers
                .FindAsync(id);

            return scrubber;
        }

        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
