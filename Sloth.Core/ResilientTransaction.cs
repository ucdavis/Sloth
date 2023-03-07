using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Sloth.Core
{
    // Use of an EF Core resiliency strategy when using multiple DbContext calls
    // within an explicit BeginTransaction():
    // https://learn.microsoft.com/ef/core/miscellaneous/connection-resiliency
    public static class ResilientTransaction
    {
        public static async Task ExecuteAsync(DbContext dbContext, Func<IDbContextTransaction, Task> action)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                await action(transaction);
                await transaction.CommitAsync();
            });
        }

        public static async Task<T> ExecuteAsync<T>(DbContext dbContext, Func<IDbContextTransaction, Task<T>> action)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            T result = default;
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                result = await action(transaction);
                await transaction.CommitAsync();
            });
            return result;
        }
    }
}
