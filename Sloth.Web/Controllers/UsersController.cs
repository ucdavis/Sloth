using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;

namespace Sloth.Web.Controllers
{
    public class UsersController : SuperController
    {
        private readonly UserManager<User> _userManager;
        private readonly SlothDbContext _context;

        public UsersController(UserManager<User> userManager, SlothDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Manage()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.ApiKeys)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return View(user);
        }

        public async Task<IActionResult> CreateNewApiKey()
        {
            // fetch user from db
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            // create new key
            var apiKey = new ApiKey();

            // associate key and update db
            user.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();

            return new JsonResult(apiKey);
        }

        public async Task<IActionResult> RevokeApiKey(string id)
        {
            // fetch user from db
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.ApiKeys)
                .FirstOrDefaultAsync(u => u.Id == userId);

            // look for key on user
            var apiKey = user.ApiKeys.FirstOrDefault(k => k.Id == id);
            if (apiKey == null)
            {
                return NotFound();
            }

            // set revoke on key
            apiKey.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
    }
}
