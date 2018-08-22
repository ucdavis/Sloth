using System;
using Sloth.Core.Models;
using Sloth.Xml;
using Sloth.Xml.Types;

namespace Sloth.Core.Extensions
{
    public static class TransferExtensions
    {
        public static Entry ToEntry(this Transfer transfer)
        {
            var direction = transfer.Direction == Transfer.CreditDebit.Credit
                ? TransactionDebitCreditCode.Credit
                : TransactionDebitCreditCode.Debit;

            var result = new Entry()
            {
                OriginCode      = transfer.Transaction.OriginCode,
                Chart           = transfer.Chart,
                Account         = transfer.Account,
                SubAccount      = transfer.SubAccount,
                ObjectCode      = transfer.ObjectCode,
                SubObjectCode   = transfer.SubObjectCode,
                DocumentNumber  = transfer.Transaction.DocumentNumber,
                TrackingNumber  = transfer.Transaction.KfsTrackingNumber,
                Amount          = transfer.Amount,
                DebitCredit     = direction,
                FiscalYear      = transfer.FiscalYear,
                SequenceNumber  = transfer.SequenceNumber,
                Description     = transfer.Description,
                BalanceType     = FinancialBalanceTypeCode.AC,
                DocType         = GetDocumentType(transfer.Transaction.DocumentType),
                TransactionDate = transfer.Transaction.TransactionDate,
            };

            if (transfer.FiscalPeriod.HasValue)
            {
                result.FiscalPeriod = GetFiscalPeriod(transfer.FiscalPeriod.Value);
            }

            return result;
        }

        private static FinancialDocumentTypeCode GetDocumentType(string docType)
        {
            if (string.IsNullOrWhiteSpace(docType))
            {
                throw new ArgumentNullException(nameof(docType));
            }

            if (string.Equals("GLIB", docType, StringComparison.InvariantCultureIgnoreCase))
            {
                return FinancialDocumentTypeCode.GLIB;
            }

            if (string.Equals("GLJV", docType, StringComparison.InvariantCultureIgnoreCase))
            {
                return FinancialDocumentTypeCode.GLJV;
            }

            throw new ArgumentException($"Unsupported DocType: {docType}", nameof(docType));
        }

        private static UniversityFiscalPeriodCode GetFiscalPeriod(int fiscalPeriod)
        {
            switch (fiscalPeriod)
            {
                case 1:
                    return UniversityFiscalPeriodCode.Period01;
                case 2:
                    return UniversityFiscalPeriodCode.Period02;
                case 3:
                    return UniversityFiscalPeriodCode.Period03;
                case 4:
                    return UniversityFiscalPeriodCode.Period04;
                case 5:
                    return UniversityFiscalPeriodCode.Period05;
                case 6:
                    return UniversityFiscalPeriodCode.Period06;
                case 7:
                    return UniversityFiscalPeriodCode.Period07;
                case 8:
                    return UniversityFiscalPeriodCode.Period08;
                case 9:
                    return UniversityFiscalPeriodCode.Period09;
                case 10:
                    return UniversityFiscalPeriodCode.Period10;
                case 11:
                    return UniversityFiscalPeriodCode.Period11;
                case 12:
                    return UniversityFiscalPeriodCode.Period12;
                case 13:
                    return UniversityFiscalPeriodCode.Period13;
            }

            return UniversityFiscalPeriodCode.None;
        }
    }
}
