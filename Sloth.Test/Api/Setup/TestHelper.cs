using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Sloth.Core;
using Sloth.Core.Models;

public static class TestHelper
{
    public static HttpClient GetAuthenticatedSlothApiClient(SlothApi app)
    {
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
        client.DefaultRequestHeaders.Add("X-Auth-Token", DbTestData.ApiKey);

        return client;
    }
    public static async Task InitializeCommonDatabase(SlothApi app)
    {
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
    }
}
