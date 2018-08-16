using System;
using Sloth.Core.Models;
using Sloth.Xml;
using Sloth.Xml.Types;

namespace Sloth.Core.Extensions
{
    public static class TransferExtensions
    {
        public static EntryWithDetail ToEntry(this Transfer transfer)
        {
            var direction = transfer.Direction == Transfer.CreditDebit.Credit
                ? transactionDebitCreditCode.C
                : transactionDebitCreditCode.D;

            var result = new EntryWithDetail()
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
                BalanceType     = financialBalanceTypeCode.AC,
                TransactionDate = transfer.Transaction.TransactionDate,
            };

            if (transfer.FiscalPeriod.HasValue)
            {
                result.FiscalPeriod = GetFiscalPeriod(transfer.FiscalPeriod.Value);
            }

            return result;
        }

        private static universityFiscalPeriodCode GetFiscalPeriod(int fiscalPeriod)
        {
            switch (fiscalPeriod)
            {
                case 1:
                    return universityFiscalPeriodCode.Item01;
                case 2:
                    return universityFiscalPeriodCode.Item02;
                case 3:
                    return universityFiscalPeriodCode.Item03;
                case 4:
                    return universityFiscalPeriodCode.Item04;
                case 5:
                    return universityFiscalPeriodCode.Item05;
                case 6:
                    return universityFiscalPeriodCode.Item06;
                case 7:
                    return universityFiscalPeriodCode.Item07;
                case 8:
                    return universityFiscalPeriodCode.Item08;
                case 9:
                    return universityFiscalPeriodCode.Item09;
                case 10:
                    return universityFiscalPeriodCode.Item10;
                case 11:
                    return universityFiscalPeriodCode.Item11;
                case 12:
                    return universityFiscalPeriodCode.Item12;
                case 13:
                    return universityFiscalPeriodCode.Item13;
            }

            return universityFiscalPeriodCode.Item;
        }
    }
}
