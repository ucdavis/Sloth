using System;
using System.Linq;
using Sloth.Core.Models;
using Sloth.Core.Resources;

namespace Sloth.Core.Data
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
            CreateRoles(context);
            CreateTeams(context);
            CreateUsers(context);
            CreateIntegrations(context);
            CreateSources(context);
            CreateTransactions(context);
        }

        private static void CreateRoles(SlothDbContext context)
        {
            // add all roles to db
            foreach (var role in Roles.GetAllRoles())
            {
                if (context.Roles.Any(r => r.Name == role)) continue;

                context.Roles.Add(new Role()
                {
                    Name = role
                });
            }
            context.SaveChanges();
        }

        private static void CreateTeams(SlothDbContext context)
        {
            if (!context.Teams.Any(t => t.Name == "John's Team"))
            {
                var anlab = new Team()
                {
                    Name = "John's Team"
                };
                context.Teams.Add(anlab);
            }
            context.SaveChanges();
        }

        private static void CreateUsers(SlothDbContext context)
        {
            if (context.Users.Any()) return;

            var users = new[]
            {
                new User()
                {
                    UserName = "jpknoll",
                    Email = "jpknoll@ucdavis.edu",
                    ApiKeys = new[] {new ApiKey()
                    {
                        Key = "TestKey123",
                        Issued = DateTime.UtcNow
                    }},
                    Roles = new []
                    {
                        new UserRole()
                        {
                            Role = context.Roles.FirstOrDefault(r => r.Name == Roles.SystemAdmin)
                        }, 
                    },
                    UserTeamRoles = new []
                    {
                        new UserTeamRole()
                        {
                            Team = context.Teams.FirstOrDefault(t => t.Name == "John's Team"),
                            Role = context.Roles.FirstOrDefault(r => r.Name == Roles.Admin),
                        }, 
                    }
                },
            };
            context.Users.AddRange(users);
            context.SaveChanges();
        }

        private static void CreateIntegrations(SlothDbContext context)
        {
            if (context.Integrations.Any()) return;

            var integrations = new[]
            {
                new Integration()
                {
                    Team = context.Teams.FirstOrDefault(t => t.Name == "John's Team"),
                    MerchantId = "ucdavis_jpknoll",
                    ReportUsername = "sloth_report",
                    ReportPasswordKey = "Report-Test-1",
                    DefaultAccount = "DEFAULT",
                    Type = Integration.IntegrationType.CyberSource
                },
            };
            context.Integrations.AddRange(integrations);
            context.SaveChanges();
        }

        private static void CreateSources(SlothDbContext context)
        {
            if (context.Sources.Any()) return;

            var sources = new[]
            {
                new Source
                {
                    Name = "ANLAB",
                    Type = "Recharge",
                    OriginCode = "AN",
                    DocumentType = DocumentTypes.GLIB,
                    Team = context.Teams.FirstOrDefault(t => t.Name == "John's Team")
                },
                new Source
                {
                    Name = "ANLAB",
                    Type = "Income",
                    OriginCode = "AN",
                    DocumentType = DocumentTypes.GLJV,
                    Team = context.Teams.FirstOrDefault(t => t.Name == "John's Team")
                }
            };
            context.Sources.AddRange(sources);
            context.SaveChanges();
        }

        private static void CreateTransactions(SlothDbContext context)
        {
            if (context.Transactions.Any()) return;

            var transactions = new[]
            {
                new Transaction()
                {
                    Creator                 = context.Users.FirstOrDefault(u => u.UserName == "jpknoll"),
                    Source                  = context.Sources.FirstOrDefault(s => s.Name == "ANLAB" && s.Type == "Recharge"),
                    Status                  = TransactionStatuses.Scheduled,
                    MerchantTrackingNumber  = "ORDER-10",
                    ProcessorTrackingNumber = "123456",
                    KfsTrackingNumber       = "TESTTHIS1",
                    TransactionDate         = DateTime.Today.AddDays(-1),
                    DocumentNumber          = "ADOCUMENT1",
                    Transfers = new []
                    {
                        new Transfer()
                        {
                            Amount      = 100,
                            Chart       = "3",
                            Account     = "6620001",
                            ObjectCode  = "7259",
                            Description = "Some useful description",
                            Direction   = Transfer.CreditDebit.Debit,
                        },
                        new Transfer()
                        {
                            Amount      = 100,
                            Chart       = "3",
                            Account     = "1010280",
                            ObjectCode  = "0299",
                            Description = "Test Clearing",
                            Direction   = Transfer.CreditDebit.Credit,
                        },
                    }
                }
            };
            context.Transactions.AddRange(transactions);
            context.SaveChanges();
        }
    }
}
