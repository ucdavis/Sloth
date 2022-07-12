using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sloth.Core.Configuration;
using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Validation;

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
            var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(financialSegmentString);

            if (segmentStringType == FinancialChartStringType.Gl)
            {
                var result =
                    await _aggieClient.GlValidateChartstring.ExecuteAsync(financialSegmentString, validateCVRs);

                var data = result.ReadData();

                return data.GlValidateChartstring.ValidationResponse.Valid;
            }

            if (segmentStringType == FinancialChartStringType.Ppm)
            {
                // TODO: validate PPM once it's available in the API
                throw new NotImplementedException();
            }

            return false;
        }
    }
}
