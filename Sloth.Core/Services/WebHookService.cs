using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        Task SendWebHooksForTeam(Team team, WebHookPayload payload);

        Task<HttpResponseMessage> SendWebHook(WebHook webHook, WebHookPayload webHookPayload, bool persist);

        Task ResendPendingWebHookRequests();
    }

    public class WebHookService : IWebHookService
    {
        private readonly SlothDbContext _dbContext;

        public WebHookService(SlothDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendWebHooksForTeam(Team team, WebHookPayload payload)
        {
            // fetch webhooks for this team and ship the payload
            var hooks = await _dbContext.WebHooks.Where(w => w.Team.Id == team.Id).ToListAsync();

            var millisecondsDelay = 1000;

            foreach (var hook in hooks)
            {
                try
                {
                    await SendWebHook(hook, payload, true);
                }
                catch (Exception)
                {
                    // Okay to swallow exception since it has already been logged.  Also important to call
                    // SendWebHook on all hooks to ensure requests are persisted.

                    // apply increasing delay after each failed call
                    await Task.Delay(millisecondsDelay);
                    millisecondsDelay = Math.Max(millisecondsDelay * 2, 32_000);
                }
            }
        }

        public async Task<HttpResponseMessage> SendWebHook(WebHook webHook, WebHookPayload payload, bool persist)
        {
            var payloadData = JsonConvert.SerializeObject(payload);

            if (!persist)
                return await SendHttpRequest(webHook, payloadData);

            var webHookRequest = new WebHookRequest
            {
                WebHookId = webHook.Id,
                LastRequestDate = DateTime.UtcNow,
                Payload = payloadData,
                RequestCount = 1
            };

            _dbContext.WebHookRequests.Add(webHookRequest);
            await _dbContext.SaveChangesAsync();

            return await SendPersistentWebHookRequest(webHook, webHookRequest);
        }

        public async Task ResendPendingWebHookRequests()
        {
            var pendingRequests = await _dbContext.WebHookRequests
                .Where(r => r.ResponseStatus != 200)
                .Include(r => r.WebHook)
                .ToListAsync();

            var millisecondsDelay = 1000;

            foreach (var webHookRequest in pendingRequests)
            {
                try
                {
                    await SendPersistentWebHookRequest(webHookRequest.WebHook, webHookRequest);
                }
                catch (Exception)
                {
                    // Okay to swallow exception since it has already been logged.  Also important to call
                    // SendWebHook on all hooks to ensure requests are persisted.

                    // apply increasing delay after each failed call
                    await Task.Delay(millisecondsDelay);
                    millisecondsDelay = Math.Max(millisecondsDelay * 2, 32_000);
                }
            }
        }

        private async Task<HttpResponseMessage> SendPersistentWebHookRequest(WebHook webHook, WebHookRequest webHookRequest)
        {
            try
            {
                var httpResponse = await SendHttpRequest(webHook, webHookRequest.Payload);

                webHookRequest.ResponseStatus = (int) httpResponse.StatusCode;
                webHookRequest.ResponseBody = await httpResponse.Content.ReadAsStringAsync();

                return httpResponse;
            }
            catch (Exception ex)
            {
                const string errorMessage = "An error occurred while calling WebHook";

                webHookRequest.ResponseStatus = (int) HttpStatusCode.InternalServerError;
                webHookRequest.ResponseBody = errorMessage;

                Log.ForContext("webhook", webHook, true)
                    .ForContext("request", webHookRequest, true)
                    .Error(ex, errorMessage);

                throw;
            }
            finally
            {
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task<HttpResponseMessage> SendHttpRequest(WebHook webHook, string payloadData)
        {
            using var client = new HttpClient();

            var body = new StringContent(payloadData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(webHook.Url, body);

            // log response
            Log.ForContext("webhook", webHook, true)
                .ForContext("response", response, true)
                .Information("Sent webhook");

            return response;
        }
    }
}
