using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
            context.Database.Migrate();

            if (!context.Users.Any())
            {
                CreateUsers(context);
            }

            if (!context.Scrubbers.Any())
            {
                CreateScrubbers(context);
            }

            context.SaveChanges();
        }

        private static void CreateScrubbers(SlothDbContext context)
        {
            var scrubbers = new[]
            {
                new Scrubber()
                {
                    Chart = "3",
                    OrganizationCode = "ACCT",
                    BatchDate = DateTime.Today,
                    BatchSequenceNumber = 1,
                    CampusCode = "DV",
                    ContactUserId = "jpknoll",
                    ContactEmail = "jpknoll@ucdavis.edu",
                    ContactAddress = "Mrak",
                    ContactDepartment = "CRU",
                    ContactPhone = "5307540708",
                    Transactions = new[]
                    {
                        new Transaction()
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
                        new Transaction()
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
                },
            };
            context.Scrubbers.AddRange(scrubbers);
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
