using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sloth.Api.Controllers
{
    [Route("api/[controller]")]
    public class ScrubbersController : Controller
    {
        private readonly SlothDbContext _context;

        public ScrubbersController(SlothDbContext context)
        {
            _context = context;
        }

        // GET: api/values
        [HttpGet]
        public async Task<List<Scrubber>> Get()
        {
            var scrubbers = await _context.Scrubbers
                .Take(1)
                .ToListAsync();

            return scrubbers;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<Scrubber> Get(int id)
        {
            var scrubber = await _context.Scrubbers
                .FindAsync(id);

            return scrubber;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
