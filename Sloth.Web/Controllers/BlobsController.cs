using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using Sloth.Core;
using Sloth.Core.Extensions;
using Sloth.Core.Models;
using Sloth.Core.Models.WebHooks;
using Sloth.Core.Resources;
using Sloth.Core.Services;
using Sloth.Web.Identity;
using Sloth.Web.Models.BlobViewModels;
using Sloth.Web.Resources;

namespace Sloth.Web.Controllers
{

    public class BlobsController : SuperController
    {
        private readonly IWebHookService _webHookService;
        private readonly IStorageService _storageService;

        public BlobsController(ApplicationUserManager userManager, SlothDbContext dbContext, IWebHookService webHookService,
            IStorageService storageService) : base(userManager, dbContext)
        {
            _webHookService = webHookService;
            _storageService = storageService;
        }

        [Authorize(Policy = PolicyCodes.TeamAnyRole)]
        public async Task<IActionResult> Details(string id)
        {
            var blob = await DbContext.Blobs
                .Where(b => b.Scrubbers.Any(s => s.Source.Team.Slug == TeamSlug)
                    || b.TransactionBlobs.Any(tb => tb.Transaction.Source.Team.Slug == TeamSlug))
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (blob == null)
            {
                ErrorMessage = "Blob not found.";
                return RedirectToAction(nameof(HomeController.TeamIndex), "Home", new { team = TeamSlug });
            }

            using var reader = new StreamReader(await _storageService.GetBlobAsync(blob.Container, blob.FileName));
            var contents = await reader.ReadToEndAsync();

            var model = new BlobDetailsViewModel()
            {
                Blob = blob,
                Contents = blob.MediaType switch
                {
                    "application/xml" => contents.XmlPrettify(),
                    "application/json" => contents.JsonPrettify(),
                    _ => contents
                }
            };

            return View(model);
        }
    }
}
