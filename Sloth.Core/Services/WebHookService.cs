using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using Sloth.Core.Models;
using Sloth.Core.Models.WebHooks;

namespace Sloth.Core.Services
{
    public interface IWebHookService
    {
        Task SendBankReconcileWebHook(Team team, BankReconcileWebHookPayload payload);

        Task<HttpResponseMessage> TestWebHook(WebHook webHook);
    }

    public class WebHookService : IWebHookService
    {
        private readonly SlothDbContext _dbContext;

        public WebHookService(SlothDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendBankReconcileWebHook(Team team, BankReconcileWebHookPayload payload)
        {
            // fetch webhooks for this team and ship the payload
            var hooks = await _dbContext.WebHooks.Where(w => w.Team.Id == team.Id).ToListAsync();

            foreach (var hook in hooks)
            {
                await SendWebHookPayload(hook, payload);
            }
        }

        public Task<HttpResponseMessage> TestWebHook(WebHook webHook)
        {
            var payload = new TestWebHookPayload()
            {
                HookId = webHook.Id,
            };
            return SendWebHookPayload(webHook, payload);
        }

        private async Task<HttpResponseMessage> SendWebHookPayload(WebHook webHook, WebHookPayload payload)
        {
            using (var client = new HttpClient())
            {
                var data = JsonConvert.SerializeObject(payload);

                var body = new StringContent(data, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(webHook.Url, body);

                // log response
                Log.ForContext("webhook", webHook, true)
                    .ForContext("response", response, true)
                    .Information("Sent webhook");

                return response;
            }
        }
    }
}
