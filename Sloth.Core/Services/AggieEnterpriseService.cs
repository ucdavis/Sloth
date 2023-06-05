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
using Serilog;
using Sloth.Core.Models;
using Sloth.Core.Extensions;

namespace Sloth.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<bool> IsAccountValid(string financialSegmentString, bool validateCVRs = true);
        Task<IGlJournalRequestResult> CreateJournal(Source source, Transaction transaction);
        Task<IGlJournalRequestStatusResult> GetJournalStatus(Guid requestId);
    }

    public class AggieEnterpriseService : IAggieEnterpriseService
    {
        private IAggieEnterpriseClient _aggieClient;
        private readonly AggieEnterpriseOptions _options;

        public AggieEnterpriseService(IOptions<AggieEnterpriseOptions> options)
        {
            _options = options.Value;
            //_aggieClient = AggieEnterpriseApi.GraphQlClient.Get(options.Value.GraphQlUrl, options.Value.Token); //If we want to use regular token
            _aggieClient = GraphQlClient.Get(_options.GraphQlUrl, _options.TokenEndpoint, _options.ConsumerKey, _options.ConsumerSecret, $"{_options.ScopeApp}-{_options.ScopeEnv}");

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

                var result =
                    await _aggieClient.PpmSegmentStringValidate.ExecuteAsync(financialSegmentString);

                var data = result.ReadData();

                return data.PpmSegmentStringValidate.ValidationResponse.Valid;
            }

            return false;
        }

        public async Task<IGlJournalRequestResult> CreateJournal(Source source, Transaction transaction)
        {
            if (transaction.Transfers.Count == 0)
            {
                throw new ArgumentException("Transaction must have at least one transfer");
            }

            var containsPpm = false;

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

                //TODO: Have a flag if this is for Devar's as they might want other fields? Or use the Journal Source?
                var glide = new GlideInput
                {
                    LineDescription = transfer.Description.SafeTruncate(100), //Line Desc is max 100 chars, our desc is currently max 40 chars
                    TransactionDate = transaction.TransactionDate.Date.ToString("yyyy-MM-dd"),
                    UdfString1      = transaction.Description.SafeTruncate(50),
                    UdfString2      = transaction.KfsTrackingNumber.StripToErpName(50),
                    UdfString3      = transaction.ProcessorTrackingNumber.SafeTruncate(50),
                    UdfString4      = transaction.MerchantTrackingNumber.SafeTruncate(50),
                    UdfString5      = transfer.Id.SafeTruncate(50)
                };

                var line = new GlJournalLineInput
                {
                    ExternalSystemIdentifier = transaction.KfsTrackingNumber.StripToGlReferenceField(10),
                    ExternalSystemReference = transfer.Id.StripToGlReferenceField(25),
                    Glide = glide
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
                    containsPpm = true;
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
                    ConsumerTrackingId = transaction.ConsumerTrackingId.SafeTruncate(80),
                    ConsumerReferenceId = source.Name.SafeTruncate(80),
                    ConsumerNotes =
                        transaction.Description.SafeTruncate(240),
                    BoundaryApplicationName = source.Name.SafeTruncate(80),
                    BatchRequest = containsPpm ? false : _options.BatchRequest // requests to promote thin ledger
                    // TODO: Seems to kill the API if specified, so don't specify for now.
                    // BatchRequest = true // always want to batch requests to promote thin ledger
                },
                Payload = new GlJournalInput
                {
                    JournalSourceName = _options.JournalSource.SafeTruncate(80),
                    JournalCategoryName = _options.JournalCategory.SafeTruncate(80),
                    JournalName = source.Name.StripToErpName(100),
                    JournalReference = source.Team.Name.StripToGlReferenceField(25),
                    AccountingDate = accountingDate?.ToString("yyyy-mm-dd"),
                    JournalLines = lines
                }
            };

            Log.ForContext("request", request).Information("Creating GL journal request for {TransactionId}",
                transaction.Id);

            var result = await _aggieClient.GlJournalRequest.ExecuteAsync(request);

            return result.ReadData();
        }

        /// <summary>
        /// Returns the status of the journal request.
        /// </summary>
        /// <param name="requestId">Found in JournalRequest.RequestId</param>
        /// <returns></returns>
        public async Task<IGlJournalRequestStatusResult> GetJournalStatus(Guid requestId)
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
