using System;
using System.Collections.Generic;
using System.Text;
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

            var fiscalPeriod = universityFiscalPeriodCode.Item;
            switch (transfer.FiscalPeriod)
            {
                case 1:
                    fiscalPeriod = universityFiscalPeriodCode.Item01;
                    break;
                case 2:
                    fiscalPeriod = universityFiscalPeriodCode.Item02;
                    break;
                case 3:
                    fiscalPeriod = universityFiscalPeriodCode.Item03;
                    break;
                case 4:
                    fiscalPeriod = universityFiscalPeriodCode.Item04;
                    break;
                case 5:
                    fiscalPeriod = universityFiscalPeriodCode.Item05;
                    break;
                case 6:
                    fiscalPeriod = universityFiscalPeriodCode.Item06;
                    break;
                case 7:
                    fiscalPeriod = universityFiscalPeriodCode.Item07;
                    break;
                case 8:
                    fiscalPeriod = universityFiscalPeriodCode.Item08;
                    break;
                case 9:
                    fiscalPeriod = universityFiscalPeriodCode.Item09;
                    break;
                case 10:
                    fiscalPeriod = universityFiscalPeriodCode.Item10;
                    break;
                case 11:
                    fiscalPeriod = universityFiscalPeriodCode.Item11;
                    break;
                case 12:
                    fiscalPeriod = universityFiscalPeriodCode.Item12;
                    break;
                case 13:
                    fiscalPeriod = universityFiscalPeriodCode.Item13;
                    break;
            }

            return new EntryWithDetail()
            {
                OriginCode      = transfer.Transaction.OriginCode,
                Chart           = transfer.Chart,
                Account         = transfer.Account,
                SubAccount      = transfer.SubAccount,
                ObjectCode      = transfer.ObjectCode,
                SubObjectCode   = transfer.SubObjectCode,
                TrackingNumber  = transfer.Transaction.KfsTrackingNumber,
                Amount          = transfer.Amount,
                DebitCredit     = direction,
                FiscalYear      = transfer.FiscalYear,
                FiscalPeriod    = fiscalPeriod,
                BalanceType     = financialBalanceTypeCode.AC,
                TransactionDate = transfer.Transaction.TransactionDate
            };
        }
    }
}
