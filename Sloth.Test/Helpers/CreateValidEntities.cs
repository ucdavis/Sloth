using Sloth.Core.Models;
using Sloth.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sloth.Test.Helpers
{
    internal static class CreateValidEntities
    {
        public static User User(int? counter)
        {
            var rtValue = new User();
            rtValue.FullName = string.Format("FirstName{0} LastName{0}", counter);
            rtValue.Email = $"test{counter}@testy.com";
            rtValue.Id = (counter ?? 99).ToString();

            return rtValue;
        }

        public static Transaction Transaction(int? counter, List<Transfer> transfers = null, string status = null)
        {
            transfers ??= new List<Transfer>();

            var rtValue = new Transaction();
            rtValue.Id = (counter ?? 99).ToString();
            rtValue.Description = $"Transaction {counter}";
            rtValue.TransactionDate = DateTime.Now;
            rtValue.Transfers = transfers;
            rtValue.SetStatus(status ?? TransactionStatuses.Completed);
            rtValue.Source = new Source()
            {
                Team = new Team()
                {
                    Name = "Test Team",
                    Slug = "testSlug"
                }
            };
            return rtValue;
        }

        public static Transfer Transfer(Transfer.CreditDebit direction, decimal amount, int? counter)
        {
            var rtValue = new Transfer();
            rtValue.Id = (counter ?? 99).ToString();
            rtValue.Amount = amount;
            rtValue.Direction = direction;
            return rtValue;
        }
    }
}
