using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sloth.Core.Models;
using Xunit;

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
            Assert.Single(txns);
            Assert.Equal(2, txns[0].Transfers.Count);
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
            Assert.Equal(DbTestData.ProcessorTrackingNumber, txn.ProcessorTrackingNumber);
            Assert.Equal(2, txn.Transfers.Count);
        }
    }
}
