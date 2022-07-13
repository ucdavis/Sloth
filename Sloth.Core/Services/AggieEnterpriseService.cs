using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sloth.Core.Configuration;
using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Validation;
using Sloth.Core.Models;

namespace Sloth.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<bool> IsAccountValid(string financialSegmentString, bool validateCVRs = true);
        Task<IGlJournalRequestResult> CreateJournal(Source source, Transaction transaction);
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

        public async Task<IGlJournalRequestResult> CreateJournal(Source source, Transaction transaction)
        {
            if (transaction.Transfers.Count == 0)
            {
                throw new ArgumentException("Transaction must have at least one transfer");
            }

            // all accounting dates must be the same across all transfers, so we can use the first one
            var accountingDate = transaction.Transfers.First().AccountingDate;

            var lines = new List<GlJournalLineInput>();

            foreach (var transfer in transaction.Transfers)
            {
                var line = new GlJournalLineInput
                {
                    ExternalSystemIdentifier = transfer.ReferenceId,
                    ExternalSystemReference = transfer.Id
                };

                if (transfer.Direction == Transfer.CreditDebit.Credit)
                {
                    line.CreditAmount = transfer.Amount;
                }
                else
                {
                    line.DebitAmount = transfer.Amount;
                }

                var segmentString = FinancialChartValidation.GetFinancialChartStringType(transfer.FinancialSegmentString);

                if (segmentString == FinancialChartStringType.Gl)
                {
                    line.GlSegmentString = transfer.FinancialSegmentString;
                }
                else if (segmentString == FinancialChartStringType.Ppm)
                {
                    line.PpmSegmentString = transfer.FinancialSegmentString;
                }
                else
                {
                    throw new ArgumentException("Invalid financial segment string");
                }

                lines.Add(line);
            }

            var request = new GlJournalRequestInput
            {
                Header = new ActionRequestHeaderInput
                {
                    ConsumerTrackingId = transaction.Id,
                    ConsumerReferenceId = source.Name,
                    ConsumerNotes = transaction.Description.Substring(0, 240), // TODO: tbd -- link back to sloth?  short desc?  240 chars max.
                    BoundaryApplicationName = "Sloth",
                    BatchRequest = true // always want to batch requests to promote thin ledger
                },
                Payload = new GlJournalInput
                {
                    JournalSourceName = "UCD SLOTH", // TODO: centrally set, so add to config settings
                    JournalCategoryName = "UCD Recharge", // TODO: config too
                    // TODO: should we add anything for journal name/desc/ref?  how does it work with batching?
                    AccountingDate = accountingDate?.ToString("yyyy-mm-dd"),
                    JournalLines = lines
                }
            };

            var result = await _aggieClient.GlJournalRequest.ExecuteAsync(request);

            return result.ReadData();
        }
    }
}
