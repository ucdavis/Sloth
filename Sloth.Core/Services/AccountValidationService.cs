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
using Sloth.Core.Extensions;

namespace Sloth.Core.Services
{
    public class AccountValidationService
    {
        private readonly IAggieEnterpriseService _aggieEnterpriseService;
        private readonly IKfsService _kfsService;

        public AccountValidationService(IAggieEnterpriseService aggieEnterpriseService, IKfsService kfsService)
        {
            _aggieEnterpriseService = aggieEnterpriseService;
            _kfsService = kfsService;
        }

        /// <summary>
        ///  Validates the given account number across multiple financial systems.
        /// </summary>
        /// <param name="accountString">Either KFS account in 3-******* format, or Aggie Enterprise GL/PPM string</param>
        /// <param name="validateCVRs">Only applies to Aggie Enterprise strings</param>
        /// <returns></returns>
        public async Task<bool> IsAccountValid(string accountString, bool validateCVRs = true)
        {
            if (accountString.Length == 9)
            {
                // KFS account
                // break into chart and account following 3-******* format
                var chart = accountString.Substring(0, 1);
                var account = accountString.Substring(2, 7);
                return await _kfsService.IsAccountValid(chart, account);
            }
            else
            {
                // assume anything else is an Aggie Enterprise GL/PPM string

                if (FinancialChartValidation.GetFinancialChartStringType(accountString) ==
                    FinancialChartStringType.Invalid)
                {
                    // format is invalid, so don't bother validating via API
                    return false;
                }

                return await _aggieEnterpriseService.IsAccountValid(accountString, validateCVRs);
            }
        }

        public static bool IsKfsAccount(string accountString)
        {
            if(accountString.Length == 7)
            {
                return true;
            }
            if(accountString.Count(a => a == '-') <=3 )
            {
                return true;
            }
            return false;
        }
    }
}
