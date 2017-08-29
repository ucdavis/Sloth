using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Sloth.Api.Jobs.Attributes
{
    public class ProlongExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
    {
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(365);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
        }
    }
}
