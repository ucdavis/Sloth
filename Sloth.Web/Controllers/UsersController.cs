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
                .FirstOrDefaultAsync(u => u.Id == userId);

            return View(user);
        }
    }
}
