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

            CreateUsers(context);
            CreateTransactions(context);
            CreateIntegrations(context);

            context.SaveChanges();
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

        private static void CreateTransactions(SlothDbContext context)
        {
            var transactions = new[]
            {
                new Transaction()
                {
                    Creator                 = context.Users.FirstOrDefault(u => u.Username == "jpknoll"),
                    Status                  = TransactionStatus.Scheduled,
                    MerchantTrackingNumber  = "ORDER-10",
                    ProcessorTrackingNumber = "123456",
                    KfsTrackingNumber       = "TESTTHIS1",
                    TransactionDate         = DateTime.Today.AddDays(-1),
                    OriginCode              = "SL",
                    DocumentNumber          = "ADOCUMENT1",
                    Transfers = new []
                    {
                        new Transfer()
                        {
                            Amount      = 100,
                            Chart       = 3,
                            Account     = "6620001",
                            ObjectCode  = "7259",
                            Description = "Some useful description",
                            Direction   = Transfer.CreditDebit.Debit,
                        },
                        new Transfer()
                        {
                            Amount      = 100,
                            Chart       = 3,
                            Account     = "1010280",
                            ObjectCode  = "0299",
                            Description = "Test Clearing",
                            Direction   = Transfer.CreditDebit.Credit,
                        },
                    }
                }
            };
            context.Transactions.AddRange(transactions);
        }

        private static void CreateIntegrations(SlothDbContext context)
        {
            var integrations = new[]
            {
                new Integration()
                {
                    MerchantId = "ucdavis_jpknoll",
                    ReportUsername = "sloth_report",
                    ReportPasswordKey = "Report-Test-1",
                    DefaultAccount = "DEFAULT",
                    Type = Integration.IntegrationType.CyberSource
                },
            };
            context.Integrations.AddRange(integrations);
        }
    }
}
