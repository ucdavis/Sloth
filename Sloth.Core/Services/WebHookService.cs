using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Sloth.Core.Configuration;
using Sloth.Core.Models;
using Sloth.Core.Models.WebHooks;

namespace Sloth.Core.Services
{
    public interface IWebHookService
    {
        Task<List<WebHookRequest>> SendWebHooksForTeam(Team team, WebHookPayload payload, bool persist = true);

        Task<WebHookRequest> SendWebHook(WebHook webHook, WebHookPayload webHookPayload, bool persist = true);

        Task<List<WebHookRequest>> ResendPendingWebHookRequests();
    }

    public class WebHookService : IWebHookService
    {
        private readonly SlothDbContext _dbContext;
        private readonly WebHookServiceOptions _options;

        public WebHookService(SlothDbContext dbContext, IOptions<WebHookServiceOptions> options)
        {
            _dbContext = dbContext;
            _options = options.Value;
        }

        public async Task<List<WebHookRequest>> SendWebHooksForTeam(Team team, WebHookPayload payload, bool persist = true)
        {
            // fetch webhooks for this team and ship the payload
            var hooks = await _dbContext.WebHooks.Where(w => w.Team.Id == team.Id && w.IsActive).ToListAsync();

            var retryDelay = _options.RetryDelaySeconds * 1000;

            var requests = new List<WebHookRequest>();

            foreach (var hook in hooks)
            {
                try
                {
                    var request = await SendWebHook(hook, payload, persist);
                    requests.Add(request);
                }
                catch (Exception)
                {
                    // Okay to swallow exception since it has already been logged.  Also important to call
                    // SendWebHook on all hooks to ensure requests are persisted.

                    // apply increasing delay after each failed call
                    await Task.Delay(retryDelay);
                    retryDelay = Math.Max(retryDelay * 2, _options.MaxRetryDelaySeconds * 1000);
                }
            }

            return requests;
        }

        public async Task<WebHookRequest> SendWebHook(WebHook webHook, WebHookPayload payload, bool persist = true)
        {
            var payloadData = JsonConvert.SerializeObject(payload);

            var webHookRequest = new WebHookRequest
            {
                WebHookId = webHook.Id,
                LastRequestDate = DateTime.UtcNow,
                Payload = payloadData,
                RequestCount = 0,
                Persist = persist
            };

            if (!webHook.IsActive)
            {
                webHookRequest.ResponseStatus = (int) HttpStatusCode.BadRequest;
                webHookRequest.ResponseBody = "Requested WebHook is not enabled";

                _dbContext.WebHookRequests.Add(webHookRequest);
                await _dbContext.SaveChangesAsync();
                return webHookRequest;
            }

            _dbContext.WebHookRequests.Add(webHookRequest);
            await _dbContext.SaveChangesAsync();

            await SendWebHookRequest(webHook, webHookRequest);

            return webHookRequest;
        }

        public async Task<List<WebHookRequest>> ResendPendingWebHookRequests()
        {
            var pendingRequests = await _dbContext.WebHookRequests
                .Where(r => r.ResponseStatus != 200 && r.Persist && r.WebHook.IsActive)
                .Include(r => r.WebHook)
                .ToListAsync();

            var retryDelay = _options.RetryDelaySeconds * 1000;

            foreach (var webHookRequest in pendingRequests)
            {
                try
                {
                    await SendWebHookRequest(webHookRequest.WebHook, webHookRequest);
                }
                catch (Exception)
                {
                    // Okay to swallow exception since it has already been logged.  Also important to call
                    // SendWebHook on all hooks to ensure requests are persisted.

                    // apply increasing delay after each failed call
                    await Task.Delay(retryDelay);
                    retryDelay = Math.Max(retryDelay * 2, _options.MaxRetryDelaySeconds * 1000);
                }
            }

            return pendingRequests;
        }

        private async Task SendWebHookRequest(WebHook webHook, WebHookRequest webHookRequest)
        {
            try
            {
                webHookRequest.RequestCount = (webHookRequest.RequestCount ?? 0) + 1;

                using var client = new HttpClient();

                var body = new StringContent(webHookRequest.Payload, Encoding.UTF8, "application/json");

                var httpResponse = await client.PostAsync(webHook.Url, body);

                // log response
                Log.ForContext("webhook", webHook, true)
                    .ForContext("response", httpResponse, true)
                    .Information("Sent webhook");

                webHookRequest.ResponseStatus = (int) httpResponse.StatusCode;
                webHookRequest.ResponseBody = await httpResponse.Content.ReadAsStringAsync();
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
    }
}
