using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(SlothDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Recreate()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// Create starter data
        /// </summary>
        public async Task Initialize()
        {
            // create identity roles
            if (!_context.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.SystemAdmin));
            }

            // create team roles
            if (!_context.TeamRoles.Any())
            {
                _context.TeamRoles.Add(new TeamRole() {Name = TeamRole.Admin});
                _context.TeamRoles.Add(new TeamRole() {Name = TeamRole.Approver});
            }

            // create system users
            if (!_context.Users.Any())
            {
                var users = new[]
                {
                    new User()
                    {
                        UserName = "jpknoll",
                        Email = "jpknoll@ucdavis.edu",
                        FullName = "John Knoll",

                    },
                };

                foreach (var user in users)
                {
                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, Roles.SystemAdmin);
                }
            }

            _context.SaveChanges();
        }

        public void CreateTestSources()
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
                    ClearingAccount   = "DEFAULT",
                    HoldingAccount    = "DEFAULT",
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
