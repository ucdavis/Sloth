using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Services;

namespace Sloth.Web.Controllers
{
    public class UsersController : SuperController
    {
        private readonly UserManager<User> _userManager;
        private readonly SlothDbContext _context;
        private readonly IDirectorySearchService _directorySearchService;

        public UsersController(UserManager<User> userManager, SlothDbContext context, IDirectorySearchService directorySearchService)
        {
            _userManager = userManager;
            _context = context;
            _directorySearchService = directorySearchService;
        }

        public async Task<IActionResult> Manage()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            return View(user);
        }

        public async Task<IActionResult> FindUser(string query)
        {
            // check database first
            var dbUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == query || u.Email == query);

            if (dbUser != null)
            {
                return new JsonResult(new
                {
                    success = true,
                    exists = true,
                    user = new
                    {
                        id       = dbUser.Id,
                        username = dbUser.UserName,
                        email    = dbUser.Email,
                        fullname = dbUser.FullName,
                    },
                });
            }

            // check directory
            var directoryUser =
                   await _directorySearchService.GetByKerberos(query)
                ?? await _directorySearchService.GetByEmail(query);

            if (directoryUser != null)
            {
                return new JsonResult(new
                {
                    success = true,
                    exists = false,
                    user = new
                    {
                        username = directoryUser.Kerberos,
                        email = directoryUser.Email,
                        fullname = directoryUser.FullName,
                    },
                });
            }

            return NotFound(new
            {
                success = false,
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserFromDirectory(string query)
        {
            // check database first
            var dbUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == query || u.Email == query);

            // user already exists
            if (dbUser != null)
            {
                return new JsonResult(new
                {
                    success = true,
                    exists = true,
                    user = new
                    {
                        id       = dbUser.Id,
                        username = dbUser.UserName,
                        email    = dbUser.Email,
                        fullname = dbUser.FullName,
                    },
                });
            }

            // find directory user
            var directoryUser =
                await _directorySearchService.GetByKerberos(query)
                ?? await _directorySearchService.GetByEmail(query);

            // can't find directory user
            if (directoryUser == null)
            {
                return NotFound(new
                {
                    success = false,
                });
            }

            // create user
            var user = new User
            {
                UserName = directoryUser.Kerberos,
                Email = directoryUser.Email,
                FullName = directoryUser.FullName,
            };
            await _userManager.CreateAsync(user);

            return new JsonResult(new
            {
                success = true,
                user = new
                {
                    id       = user.Id,
                    username = user.UserName,
                    email    = user.Email,
                    fullname = user.FullName,
                }
            });
        }
    }
}
