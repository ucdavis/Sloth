using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Web.Identity;
using Sloth.Web.Models.WebHookViewModels;

namespace Sloth.Web.Controllers
{
    public class WebHooksController : SuperController
    {
        public WebHooksController(ApplicationUserManager userManager, SlothDbContext dbContext)
            : base(userManager, dbContext)
        {
        } 

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Sources = await DbContext.Sources
                .Where(s => s.Team.Slug == TeamSlug)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateWebHookViewModel model)
        {
            // TODO: validate model
            var adminTeams = await GetUsersAdminTeams();
            var team = adminTeams.FirstOrDefault(t => t.Slug == TeamSlug);
            if (team == null)
            {
                return View(model);
            }

            // create webhook
            var target = new WebHook
            {
                Url         = model.Url,
                ContentType = model.ContentType,
                Team        = team,
            };

            DbContext.WebHooks.Add(target);
            await DbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Teams", new { id = team.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var webhook = await DbContext.WebHooks.FirstOrDefaultAsync(i => i.Id == id);

            var model = new EditWebHookViewModel()
            {
                Url = webhook.Url,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, EditWebHookViewModel model)
        {
            var webhook = await DbContext.WebHooks.FirstOrDefaultAsync(i => i.Id == id);
            if (webhook == null)
            {
                return NotFound();
            }

            // validate model
            var adminTeams = await GetUsersAdminTeams();
            var team = adminTeams.FirstOrDefault(t => t.Slug == TeamSlug);
            if (team == null)
            {
                return View(model);
            }

            // update webhook
            webhook.ContentType = model.ContentType;
            webhook.Url = model.Url;

            await DbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Teams", new { id = team.Id });
        }
    }
}