using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;

public class DbTestData
{
    public static string ApiKey = "d16f64a1-aecc-4482-96d7-b78cb639b456";
    public static string Team = "ANLAB";
    public static string ProcessorTrackingNumber = "123456";

    private readonly SlothDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DbTestData(SlothDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
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
            _context.TeamRoles.Add(new TeamRole() { Name = TeamRole.Admin });
            _context.TeamRoles.Add(new TeamRole() { Name = TeamRole.Approver });
        }

        // create system users
        if (!_context.Users.Any())
        {
            var users = new[]
            {
                    new User()
                    {
                        UserName = "jsylvest",
                        Email = "jsylvestre@ucdavis.edu",
                        FullName = "Jason Sylvestre",
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

    public void CreateTeamAndApiKey()
    {
        var team = new Team()
        {
            Name = "ANLAB",
            Slug = "anlab",
            KfsContactDepartmentName = "ANLAB",
            KfsContactEmail = "anlab@mail.com",
            KfsContactPhoneNumber = "555-555-5555",
            KfsContactMailingAddress = "123 Main St",
            KfsContactUserId = "jsylvest",
        };

        team.ApiKeys.Add(
            new ApiKey
            {
                Issued = DateTime.Now,
                Key = ApiKey,
                Team = team,
            }
        );

        _context.Teams.Add(team);
        _context.SaveChanges();
    }

    public void CreateTestSources()
    {
        if (_context.Sources.Any()) return;

        var sources = new[]
        {
                new Source
                {
                    Chart = "3",
                    OrganizationCode = "ALAB",
                    Name = "ANLAB",
                    Type = "Recharge",
                    OriginCode = "AN",
                    DocumentType = DocumentTypes.GLIB,
                    Team = _context.Teams.FirstOrDefault(t => t.Name == "ANLAB")
                },
                new Source
                {
                    Chart = "3",
                    OrganizationCode = "ALAB",
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

    public void CreateTestTransactions()
    {
        if (_context.Transactions.Any()) return;

        var transactions = new[]
        {
                new Transaction()
                {
                    Creator                 = _context.Users.FirstOrDefault(u => u.UserName == "jsylvest"),
                    Source                  = _context.Sources.FirstOrDefault(s => s.Name == "ANLAB" && s.Type == "Recharge"),
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
                }.SetStatus(TransactionStatuses.Scheduled)
            };
        _context.Transactions.AddRange(transactions);
        _context.SaveChanges();
    }
}
