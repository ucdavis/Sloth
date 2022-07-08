using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sloth.Core.Configuration;
using Sloth.Core.Extensions;
using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;

namespace Sloth.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<bool> IsAccountValid(string financialSegmentString, bool validateCVRs = true);
    }

    public class AggieEnterpriseService : IAggieEnterpriseService
    {
        private readonly IAggieEnterpriseClient _aggieClient;

        public AggieEnterpriseService(IOptions<AggieEnterpriseOptions> options)
        {
            _aggieClient = AggieEnterpriseApi.GraphQlClient.Get(options.Value.GraphQlUrl, options.Value.Token);
        }

        public async Task<bool> IsAccountValid(string financialSegmentString, bool validateCVRs = true)
        {
            // TODO -- should we first determine if structure is correct before calling api?

            // TODO -- determine if financial string is GL or PPM and call the correct API (or update AE nuget to do it?)
            // TODO: assuming GL string for now
            var result = await _aggieClient.GlValidateChartstring.ExecuteAsync(financialSegmentString, validateCVRs);

            var data = result.ReadData();

            return data.GlValidateChartstring.ValidationResponse.Valid;
        }
    }
}
