using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sloth.Core.Configuration;
using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;
using Sloth.Core.Models;
using Sloth.Core.Extensions;

namespace Sloth.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<bool> IsAccountValid(string financialSegmentString, bool validateCVRs = true);
        Task<IGlJournalRequestResult> CreateJournal(Source source, Transaction transaction);
        Task<IGlJournalRequestStatusResult> GetJournalStatus(string requestId);
    }

    public class AggieEnterpriseService : IAggieEnterpriseService
    {
        private readonly IAggieEnterpriseClient _aggieClient;
        private readonly string _journalSource;
        private readonly string _journalCategory;

        public AggieEnterpriseService(IOptions<AggieEnterpriseOptions> options)
        {
            _aggieClient = AggieEnterpriseApi.GraphQlClient.Get(options.Value.GraphQlUrl, options.Value.Token);

            _journalSource = options.Value.JournalSource;
            _journalCategory = options.Value.JournalCategory;
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
                // there is no validate ppm string, but we can validate by segments
                var segments = FinancialChartValidation.GetPpmSegments(financialSegmentString);

                var result =
                    await _aggieClient.PpmSegmentsValidate.ExecuteAsync(ConvertToPpmSegmentInput(segments),
                        accountingDate: null);

                var data = result.ReadData();

                return data.PpmSegmentsValidate.ValidationResponse.Valid;
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
                // first, we need to re-verify the FinancialSegmentString is valid
                var isValid = await IsAccountValid(transfer.FinancialSegmentString);

                if (!isValid)
                {
                    throw new ArgumentException("Invalid financial segment string: " + transfer.FinancialSegmentString);
                }

                var line = new GlJournalLineInput
                {
                    ExternalSystemIdentifier = transfer.ReferenceId,
                    ExternalSystemReference = transfer.Id.Substring(0, 25) // max length is 25 so need to cutoff
                };

                if (transfer.Direction == Transfer.CreditDebit.Credit)
                {
                    line.CreditAmount = transfer.Amount;
                }
                else
                {
                    line.DebitAmount = transfer.Amount;
                }

                var segmentStringType =
                    FinancialChartValidation.GetFinancialChartStringType(transfer.FinancialSegmentString);

                if (segmentStringType == FinancialChartStringType.Gl)
                {
                    line.GlSegmentString = transfer.FinancialSegmentString;
                }
                else if (segmentStringType == FinancialChartStringType.Ppm)
                {
                    line.PpmSegmentString = transfer.FinancialSegmentString;
                }
                else
                {
                    throw new ArgumentException("Invalid financial segment string" + transfer.FinancialSegmentString);
                }

                lines.Add(line);
            }

            var request = new GlJournalRequestInput
            {
                Header = new ActionRequestHeaderInput
                {
                    ConsumerTrackingId = transaction.Id,
                    ConsumerReferenceId = source.Name,
                    ConsumerNotes =
                        transaction.Description
                            ?.Substring(0, 240), // TODO: tbd -- link back to sloth?  short desc?  240 chars max.
                    BoundaryApplicationName = source.Name,
                    // TODO: Seems to kill the API if specified, so don't specify for now.
                    // BatchRequest = true // always want to batch requests to promote thin ledger
                },
                Payload = new GlJournalInput
                {
                    JournalSourceName = _journalSource,
                    JournalCategoryName = _journalCategory,
                    JournalName = "Sloth Recharges",
                    JournalReference = source.Team.Name.SafeRegexRemove(@"[^A-Za-z0-9_-]+").SafeTruncate(25),
                    // TODO: should we add anything for journal name/desc/ref?  how does it work with batching?
                    AccountingDate = accountingDate?.ToString("yyyy-mm-dd"),
                    JournalLines = lines
                }
            };

            var result = await _aggieClient.GlJournalRequest.ExecuteAsync(request);

            return result.ReadData();
        }

        /// <summary>
        /// Returns the status of the journal request.
        /// </summary>
        /// <param name="requestId">Found in JournalRequest.RequestId</param>
        /// <returns></returns>
        public async Task<IGlJournalRequestStatusResult> GetJournalStatus(string requestId)
        {
            var result = await _aggieClient.GlJournalRequestStatus.ExecuteAsync(requestId);

            return result.ReadData();
        }

        private PpmSegmentInput ConvertToPpmSegmentInput(PpmSegments segments)
        {
            return new PpmSegmentInput
            {
                Award = segments.Award,
                Organization = segments.Organization,
                Project = segments.Project,
                Task = segments.Task,
                ExpenditureType = segments.ExpenditureType,
                FundingSource = segments.FundingSource,
            };
        }
    }
}
