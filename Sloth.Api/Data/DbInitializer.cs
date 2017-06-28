using System;
using System.Linq;
using Sloth.Core;
using Sloth.Core.Models;

namespace Sloth.Api.Data
{
    /// <summary>
    /// Creates sample data for development environments
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Create sample data
        /// </summary>
        /// <param name="context"></param>
        public static void Initialize(SlothDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (!context.Users.Any())
            {
                CreateUsers(context);
            }

            if (!context.Transactions.Any())
            {
                CreateTransactions(context);
            }

            context.SaveChanges();
        }

        private static void CreateTransactions(SlothDbContext context)
        {
            var transactions = new[]
            {
                new Transaction()
                {
                    Creator = context.Users.FirstOrDefault(u => u.Username == "jpknoll"),
                    Status = TransactionStatus.Scheduled,
                    Transfers = new []
                    {
                        new Transfer()
                        {
                            Amount = 100,
                            Chart = 3,
                            Account = "6620001",
                            ObjectCode = "7259",
                            BalanceType = "AC",
                            DocType = "GLJV",
                            OriginCode = "92",
                            DocumentNumber = "ADOCUMENT1",
                            Description = "Some useful description",
                            TransactionDate = DateTime.Today.AddDays(-1),
                            DebitCredit = "D",
                            TrackingNumber = "TESTTHIS1"
                        },
                        new Transfer()
                        {
                            Amount = 100,
                            Chart = 3,
                            Account = "1010280",
                            ObjectCode = "0299",
                            BalanceType = "AC",
                            DocType = "GLJV",
                            OriginCode = "92",
                            DocumentNumber = "ADOCUMENT1",
                            Description = "Test Clearing",
                            TransactionDate = DateTime.Today.AddDays(-1),
                            DebitCredit = "C",
                            TrackingNumber = "*CLEARING*"
                        },
                    }
                }
            };
            context.Transactions.AddRange(transactions);
        }

        private static void CreateUsers(SlothDbContext context)
        {
            var users = new[]
            {
                new User()
                {
                    Username = "jpknoll",
                    Email = "jpknoll@ucdavis.edu",
                    Keys = new[] {new ApiKey() { Id = "TestKey123", Issued = DateTime.UtcNow }}
                },
            };
            context.Users.AddRange(users);
        }
    }
}
