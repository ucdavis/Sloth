using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Web.Models.SystemViewModels;

namespace Sloth.Web.Controllers
{
    [Authorize(Roles = Roles.SystemAdmin)]
    public class SystemController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SlothDbContext _dbContext;

        public SystemController(UserManager<User> userManager, SlothDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync(Roles.SystemAdmin);

            return View(new SystemViewModel
            {
                AdminUsers = adminUsers
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToRole(string userId, string role)
        {
            // find user
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            // add to role
            await _userManager.AddToRoleAsync(user, role);

            return new JsonResult(new
            {
                success = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromRole(string userId, string role)
        {
            
            if(role != Roles.SystemAdmin)
            {
                return BadRequest();
            }
            // find user
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.UserName == userId);
            if(user == null)
            {
                return NotFound();
            }

            // Remove from role
            await _userManager.RemoveFromRoleAsync(user, role);

            Log.Warning($"System Admin removed: user: {userId} role: {role}");

            return RedirectToAction(nameof(Index));
        }
    }
}
