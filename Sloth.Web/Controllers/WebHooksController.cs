using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Models.WebHooks;
using Sloth.Core.Services;
using Sloth.Web.Identity;
using Sloth.Web.Models.WebHookViewModels;
using Sloth.Web.Resources;

namespace Sloth.Web.Controllers
{
    [Authorize(Policy = PolicyCodes.TeamAdmin)]
    public class WebHooksController : SuperController
    {
        private readonly IWebHookService _webHookService;

        public WebHooksController(ApplicationUserManager userManager, SlothDbContext dbContext, IWebHookService webHookService)
            : base(userManager, dbContext)
        {
            _webHookService = webHookService;
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
                IsActive    = model.IsActive,
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
            var webhook = await DbContext.WebHooks
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (webhook == null)
            {
                ErrorMessage = "WebHook not found.";
                return RedirectToAction(nameof(HomeController.TeamIndex), "Home", new { team = TeamSlug });
            }

            var model = new EditWebHookViewModel()
            {
                Url = webhook.Url,
                IsActive = webhook.IsActive,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, EditWebHookViewModel model)
        {
            var webhook = await DbContext.WebHooks
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (webhook == null)
            {
                ErrorMessage = "WebHook not found.";
                return RedirectToAction(nameof(HomeController.TeamIndex), "Home", new { team = TeamSlug });
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
            webhook.IsActive = model.IsActive;

            await DbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Teams", new { id = team.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Test(string id)
        {
            var webhook = await DbContext.WebHooks.FirstOrDefaultAsync(i => i.Id == id);
            if (webhook == null)
            {
                return NotFound();
            }

            return View(webhook);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SendTest(string id, [FromQuery] bool? persist)
        {
            var webhook = await DbContext.WebHooks
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (webhook == null)
            {
                ErrorMessage = "WebHook not found.";
                return RedirectToAction(nameof(HomeController.TeamIndex), "Home", new { team = TeamSlug });
            }

            var payload = new TestWebHookPayload()
            {
                HookId = webhook.Id,
            };


            var result = await _webHookService.SendWebHook(webhook, payload, persist ?? false);

            // ship result
            return new JsonResult(new
            {
                result.ResponseStatus,
                Response = result.ResponseBody,
            });
        }
    }
}
