using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using TestHelpers.Helpers;
using Xunit;
using Sloth.Core;
using Sloth.Core.Services;
using Sloth.Web.Controllers;
using Sloth.Test.Helpers;
using Sloth.Core.Models;
using Sloth.Core.Resources;
using Sloth.Web.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Runtime.CompilerServices;

namespace Sloth.Test.Web
{
    public class TransactionControllerTests
    {
        public Mock<SlothDbContext> MockDbContext { get; set; }
        public Mock<IWebHookService> MockWebhookService { get; set; }
        public Mock<HttpContext> MockHttpContext { get; set; }
        public Mock<FakeApplicationUserManager> MockUserManager { get; set; }
        public Mock<DatabaseFacade> MockDatabase { get; set; }
        public Mock<IDbContextTransaction> MockDbContextTransaction { get; set; }
        public Mock<DbTransaction> MockDbTransaction { get; set; }
        public Mock<IAggieEnterpriseService> MockAggieEnterpriseService { get; set; }
        public Mock<IExecutionStrategy> MockExecutionStrategy { get; set; }

        public TransactionsController Controller { get; set; }


        public List<User> UserData { get; set; }
        public List<Transaction> TransactionData { get; private set; }

        public int NextId { get; set; } = 1;

        public TransactionControllerTests()
        {
            var mockTempDataSerializer = new Mock<TempDataSerializer>();
            var mockDataProvider = new Mock<SessionStateTempDataProvider>(mockTempDataSerializer.Object);

            MockDbContext = new Mock<SlothDbContext>(new DbContextOptions<SlothDbContext>());
            MockWebhookService = new Mock<IWebHookService>();
            MockUserManager = new Mock<FakeApplicationUserManager>();
            MockDatabase = new Mock<DatabaseFacade>(MockDbContext.Object);
            MockDbContextTransaction = new Mock<IDbContextTransaction>();
            MockHttpContext = new Mock<HttpContext>();
            MockDbTransaction = new Mock<DbTransaction>();
            MockAggieEnterpriseService = new Mock<IAggieEnterpriseService>();
            MockExecutionStrategy = new Mock<IExecutionStrategy>();

            //Default Data
            UserData = new List<User>();
            for (int i = 0; i < 5; i++)
            {
                var user = CreateValidEntities.User(i + 1);
                UserData.Add(user);
            }

            //Setups

            // IInfrastructure is basically an accessor for use with extension methods. We're mocking
            // it directly because extension method DbContextTransaction.GetDbTransaction() can't be mocked.
            var mockDbTransactionAccessor = MockDbContextTransaction.As<IInfrastructure<DbTransaction>>();
            mockDbTransactionAccessor.SetupGet(a => a.Instance)
                .Returns(MockDbTransaction.Object);
            MockUserManager.Setup(a => a.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(UserData[1]);
            MockDbContext.SetupGet(a => a.Database).Returns(MockDatabase.Object);
            MockDbContext.Setup(a => a.GetNextDocumentNumber(It.IsAny<DbTransaction>()))
                .Returns(Task.FromResult($"Doc{NextId++}"));
            MockDatabase.Setup(a => a.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(MockDbContextTransaction.Object);
            MockExecutionStrategy.SetupGet(a => a.RetriesOnFailure).Returns(false);
            MockExecutionStrategy.Setup(a => a.ExecuteAsync(
                It.IsAny<It.IsAnyType>(), // state
                It.IsAny<Func<DbContext, It.IsAnyType, CancellationToken, Task<bool>>>(), //operation
                It.IsAny<Func<DbContext, It.IsAnyType, CancellationToken, Task<ExecutionResult<bool>>>>(), //verifySucceeded
                It.IsAny<CancellationToken>() //cancellationToken
            ))
            .Callback((object arg1, object arg2, object arg3, object arg4)
                =>
            {
                // arg1 is a closure passed to strategy.ExecuteAsync in ResilientTransaction.ExecuteAsync
                var resilientTransactionFunc = (Func<Task>)arg1;
                // arg2 appears to be a closure defined in Microsoft.EntityFrameworkCore.ExecutionStrategyExtensions
                var execStrategyFunc = (Func<DbContext, Func<Task>, CancellationToken, Task<bool>>)arg2;
                var cancellationToken = (CancellationToken)arg4;

                execStrategyFunc.Invoke(MockDbContext.Object, resilientTransactionFunc, cancellationToken);
            })
            .Returns(Task.FromResult(true));
            MockDatabase.Setup(a => a.CreateExecutionStrategy())
                .Returns(MockExecutionStrategy.Object);
            MockDbContextTransaction.Setup(a => a.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            MockDbContextTransaction.Setup(a => a.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            MockDbContextTransaction.Setup(a => a.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);


            var routeData = new RouteData();
            routeData.Values.Add("team", "testSlug");
            Controller = new TransactionsController(MockUserManager.Object, MockDbContext.Object, MockWebhookService.Object, MockAggieEnterpriseService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = MockHttpContext.Object,
                    RouteData = routeData
                },
                TempData = new TempDataDictionary(MockHttpContext.Object, mockDataProvider.Object),
            };
        }

        [Fact]
        public async Task ReversalWithRoundingError()
        {
            //arrange
            var transactions = new List<Transaction>();
            transactions.Add(CreateValidEntities.Transaction(1, new List<Transfer>
            {
                CreateValidEntities.Transfer(Transfer.CreditDebit.Debit, 100, 1),
                CreateValidEntities.Transfer(Transfer.CreditDebit.Credit, 0.01m, 2),
                CreateValidEntities.Transfer(Transfer.CreditDebit.Credit, 0.02m, 3),
                CreateValidEntities.Transfer(Transfer.CreditDebit.Credit, 99.97m, 4),
            }, TransactionStatuses.Completed));

            Transaction reversal = null;
            var mockDbSet = transactions.AsQueryable().MockAsyncDbSet();
            mockDbSet.Setup(a => a.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .Callback((Transaction t, CancellationToken _) =>
                {
                    reversal = t;
                    //return Task.FromResult<ValueTask<EntityEntry<Transaction>>>(default);
                });

            MockDbContext.SetupGet(a => a.Transactions).Returns(mockDbSet.Object);

            //act
            var result = await Controller.CreateReversal("1", 16.67m);

            //assert
            reversal.ShouldNotBeNull();
            var totalCredit = reversal.Transfers.Where(t => t.Direction == Transfer.CreditDebit.Credit).Select(t => t.Amount);
            var totalDebit = reversal.Transfers.Where(t => t.Direction == Transfer.CreditDebit.Debit).Select(t => t.Amount);
            totalCredit.ShouldBe(totalDebit);
            Controller.Message.ShouldBe("Reversal created successfully");
        }

        [Fact]
        public async Task ReversalWithNoRoundingError()
        {
            //arrange
            var transactions = new List<Transaction>();
            transactions.Add(CreateValidEntities.Transaction(1, new List<Transfer>
            {
                CreateValidEntities.Transfer(Transfer.CreditDebit.Debit, 100, 1),
                CreateValidEntities.Transfer(Transfer.CreditDebit.Credit, 0.01m, 2),
                CreateValidEntities.Transfer(Transfer.CreditDebit.Credit, 0.01m, 3),
                CreateValidEntities.Transfer(Transfer.CreditDebit.Credit, 99.98m, 4),
            }, TransactionStatuses.Completed));

            Transaction reversal = null;
            var mockDbSet = transactions.AsQueryable().MockAsyncDbSet();
            mockDbSet.Setup(a => a.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .Callback((Transaction t, CancellationToken _) =>
                {
                    reversal = t;
                    //return Task.FromResult<ValueTask<EntityEntry<Transaction>>>(default);
                });

            MockDbContext.SetupGet(a => a.Transactions).Returns(mockDbSet.Object);

            //act
            var result = await Controller.CreateReversal("1", 16.67m);

            //assert
            reversal.ShouldNotBeNull();
            var totalCredit = reversal.Transfers.Where(t => t.Direction == Transfer.CreditDebit.Credit).Select(t => t.Amount);
            var totalDebit = reversal.Transfers.Where(t => t.Direction == Transfer.CreditDebit.Debit).Select(t => t.Amount);
            totalCredit.ShouldBe(totalDebit);
            Controller.Message.ShouldBe("Reversal created successfully");
        }
    }
}
