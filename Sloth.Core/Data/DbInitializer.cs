using System;
using System.Linq;
using System.Threading.Tasks;
using Sloth.Core.Models;
using Sloth.Core.Resources;

namespace Sloth.Core.Data
{
    /// <summary>
    /// Creates sample data for development environments
    /// </summary>
    public class DbInitializer
    {
        private readonly SlothDbContext _context;

        public DbInitializer(SlothDbContext context)
        {
            _context = context;
        }

        public async Task Recreate()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// Create starter data
        /// </summary>
        public void Initialize()
        {
            CreateRoles();
            CreateTeams();
            CreateUsers();
            CreateSources();
        }

        public void CreateRoles()
        {
            // add all roles to db
            foreach (var role in Roles.GetAllRoles())
            {
                if (_context.Roles.Any(r => r.Name == role)) continue;

                _context.Roles.Add(new Role()
                {
                    Name = role
                });
            }
            _context.SaveChanges();
        }

        public void CreateTeams()
        {
            if (!_context.Teams.Any(t => t.Name == "ANLAB"))
            {
                var anlab = new Team()
                {
                    Name = "ANLAB",
                    ApiKeys = new[] {new ApiKey()
                    {
                        Key = Guid.NewGuid().ToString(),
                        Issued = DateTime.UtcNow
                    }},
                };
                _context.Teams.Add(anlab);
            }
            _context.SaveChanges();
        }

        public void CreateUsers()
        {
            if (_context.Users.Any()) return;

            var users = new[]
            {
                new User()
                {
                    UserName = "jpknoll",
                    Email = "jpknoll@ucdavis.edu",
                    Roles = new []
                    {
                        new UserRole()
                        {
                            Role = _context.Roles.FirstOrDefault(r => r.Name == Roles.SystemAdmin)
                        }, 
                    },
                    UserTeamRoles = new []
                    {
                        new UserTeamRole()
                        {
                            Team = _context.Teams.FirstOrDefault(t => t.Name == "ANLAB"),
                            Role = _context.Roles.FirstOrDefault(r => r.Name == Roles.Admin),
                        }, 
                    }
                },
            };
            _context.Users.AddRange(users);
            _context.SaveChanges();
        }

        public void CreateSources()
        {
            if (_context.Sources.Any()) return;

            var sources = new[]
            {
                new Source
                {
                    Name = "ANLAB",
                    Type = "Recharge",
                    OriginCode = "AN",
                    DocumentType = DocumentTypes.GLIB,
                    Team = _context.Teams.FirstOrDefault(t => t.Name == "ANLAB")
                },
                new Source
                {
                    Name = "ANLAB",
                    Type = "CyberSource",
                    OriginCode = "AN",
                    DocumentType = DocumentTypes.GLJV,
                    Team = _context.Teams.FirstOrDefault(t => t.Name == "ANLAB")
                }
            };
            _context.Sources.AddRange(sources);
            _context.SaveChanges();
        }

        public void CreateTestIntegrations()
        {
            if (_context.Integrations.Any()) return;

            var integrations = new[]
            {
                new Integration()
                {
                    Team              = _context.Teams.FirstOrDefault(t => t.Name == "ANLAB"),
                    Source            = _context.Sources.FirstOrDefault(s => s.Name == "ANLAB" && s.Type == "CyberSource"),
                    MerchantId        = "ucdavis_jpknoll",
                    ReportUsername    = "sloth_report",
                    ReportPasswordKey = "Report-Test-1",
                    DefaultAccount    = "DEFAULT",
                    Type              = IntegrationTypes.CyberSource
                },
            };
            _context.Integrations.AddRange(integrations);
            _context.SaveChanges();
        }

        public void CreateTestTransactions()
        {
            if (_context.Transactions.Any()) return;

            var transactions = new[]
            {
                new Transaction()
                {
                    Creator                 = _context.Users.FirstOrDefault(u => u.UserName == "jpknoll"),
                    Source                  = _context.Sources.FirstOrDefault(s => s.Name == "ANLAB" && s.Type == "Recharge"),
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
            _context.Transactions.AddRange(transactions);
            _context.SaveChanges();
        }
    }
}
