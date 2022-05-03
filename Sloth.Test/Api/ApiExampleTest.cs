using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Xunit;

namespace Sloth.Test.Api
{
    public class ExampleTests
    {
        [Fact]
        public async Task TestGetTransactions()
        {
            await using var app = new SlothApi();

            using var scope = app.Services.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<SlothDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var dbTestInit = new DbTestData(dbContext, userManager, roleManager);

            await dbContext.Database.EnsureCreatedAsync();

            await dbTestInit.Initialize();
            dbTestInit.CreateTeamAndApiKey();
            dbTestInit.CreateTestSources();
            dbTestInit.CreateTestTransactions();

            var client = app.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", DbTestData.ApiKey);
            var response = await client.GetAsync("/v1/transactions");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var txns = JsonConvert.DeserializeObject<List<Transaction>>(await response.Content.ReadAsStringAsync());
            
            // should return our single txn with 2 transfers
            Assert.Single(txns);
            Assert.Equal(2, txns[0].Transfers.Count);
        }

        [Fact]
        public async Task TestGetTransaction()
        {
            await using var app = new SlothApi();

            using var scope = app.Services.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<SlothDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var dbTestInit = new DbTestData(dbContext, userManager, roleManager);

            await dbContext.Database.EnsureCreatedAsync();

            await dbTestInit.Initialize();
            dbTestInit.CreateTeamAndApiKey();
            dbTestInit.CreateTestSources();
            dbTestInit.CreateTestTransactions();

            var client = app.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", DbTestData.ApiKey);
            var response = await client.GetAsync("/v1/transactions/processor/" + "123456");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var txn = JsonConvert.DeserializeObject<Transaction>(await response.Content.ReadAsStringAsync());
            
            // should return our single txn with 2 transfers
            Assert.Equal("123456", txn.ProcessorTrackingNumber);
            Assert.Equal(2, txn.Transfers.Count);
        }
    }
}

// TODO: use regular db init or keep our own version just for testing?
class DbTestData
{
    public static string ApiKey = "d16f64a1-aecc-4482-96d7-b78cb639b456";

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

class SlothApi : WebApplicationFactory<Sloth.Api.Startup>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // TODO: change to SQLite https://docs.microsoft.com/en-us/ef/core/testing/testing-without-the-database#sqlite-in-memory
        var root = new InMemoryDatabaseRoot();

        var projectDir = Directory.GetCurrentDirectory();
        var configPath = Path.Combine(projectDir, "appsettings-test.json");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile(configPath, optional: false);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<SlothDbContext>));
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<SlothDbContext>()
                .AddUserManager<ApplicationUserManager>();
            services.AddDbContext<SlothDbContext>(options =>
                options.UseInMemoryDatabase("SlothTesting", root));
        });

        return base.CreateHost(builder);
    }
}

// TODO: tmp to remove dependency on web proj
public class ApplicationUserManager : UserManager<User>
{
    public ApplicationUserManager(
        IUserStore<User> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<User> passwordHasher,
        IEnumerable<IUserValidator<User>> userValidators,
        IEnumerable<IPasswordValidator<User>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<User>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    public override Task<User> FindByIdAsync(string userId)
    {
        return Users
            .Include(u => u.UserTeamRoles)
                .ThenInclude(p => p.Team)
            .Include(u => u.UserTeamRoles)
                .ThenInclude(p => p.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
}