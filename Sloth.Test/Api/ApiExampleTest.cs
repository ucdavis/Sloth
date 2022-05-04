using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sloth.Core.Models;
using Xunit;
using Shouldly;
using System;
using Sloth.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Sloth.Test.Api
{
    public class ExampleTests
    {
        [Fact]
        public async Task TestGetTransactions()
        {
            await using var app = new SlothApi();

            await TestHelper.InitializeCommonDatabase(app);

            var client = TestHelper.GetAuthenticatedSlothApiClient(app);

            var response = await client.GetAsync("/v1/transactions");

            var txns = JsonConvert.DeserializeObject<List<Transaction>>(await response.Content.ReadAsStringAsync());

            // should return our single txn with 2 transfers
            txns.ShouldNotBeNull();
            txns.Count.ShouldBe(1);
            txns[0].Transfers.Count.ShouldBe(2);
        }

        [Fact]
        public async Task TestGetTransaction()
        {
            await using var app = new SlothApi();

            await TestHelper.InitializeCommonDatabase(app);

            var client = TestHelper.GetAuthenticatedSlothApiClient(app);

            var response = await client.GetAsync("/v1/transactions/processor/" + DbTestData.ProcessorTrackingNumber);
            var txn = JsonConvert.DeserializeObject<Transaction>(await response.Content.ReadAsStringAsync());

            // should return our single txn with 2 transfers
            txn.ProcessorTrackingNumber.ShouldBe(DbTestData.ProcessorTrackingNumber);
            txn.Transfers.Count.ShouldBe(2);
        }

        [Fact(Skip = "Fix once net6")]
        public async Task TestGetTransactionFromOtherTeam()
        {
            var teamApiKey = Guid.NewGuid().ToString();

            await using var app = new SlothApi();

            await TestHelper.InitializeCommonDatabase(app);

            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SlothDbContext>();

            var team = new Team()
            {
                Name = "Alice",
                Slug = "alice",
                KfsContactDepartmentName = "Alice",
                KfsContactEmail = "alice@mail.com",
                KfsContactPhoneNumber = "555-555-5555",
                KfsContactMailingAddress = "123 Main St",
                KfsContactUserId = "jsylvest",
            };

            team.ApiKeys.Add(
                new ApiKey
                {
                    Issued = DateTime.Now,
                    Key = teamApiKey,
                    Team = team,
                }
            );

            dbContext.Teams.Add(team);
            dbContext.SaveChanges();

            var client = TestHelper.GetAuthenticatedSlothApiClient(app);

            // remove the valid auth token and replace it with the other team's token who should not see this txn
            client.DefaultRequestHeaders.Remove("X-Auth-Token");
            client.DefaultRequestHeaders.Add("X-Auth-Token", teamApiKey);

            var response = await client.GetAsync("/v1/transactions/processor/" + DbTestData.ProcessorTrackingNumber);

            // should not return anything because the other team should not have access to this txn
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
        }
    }
}
