using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sloth.Core.Models;
using Xunit;
using Shouldly;

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
    }
}
