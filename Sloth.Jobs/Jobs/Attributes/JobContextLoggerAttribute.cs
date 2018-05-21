using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Serilog;

namespace Sloth.Jobs.Jobs.Attributes
{
    public class JobContextLoggerAttribute : JobFilterAttribute,
        IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        private static readonly ILogger Logger = Log.Logger;

        public void OnCreating(CreatingContext context)
        {
            Logger.Debug("Creating a job based on method {jobName}...", context.Job.Method.Name);
        }

        public void OnCreated(CreatedContext context)
        {
            Logger.Debug(
                "Job that is based on method {jobName} has been created with id {jobId}",
                context.Job.Method.Name,
                context.BackgroundJob?.Id);
        }

        public void OnPerforming(PerformingContext context)
        {
            Logger.Debug("Starting to perform job {jobId}", context.BackgroundJob.Id);
        }

        public void OnPerformed(PerformedContext context)
        {
            Logger.Debug("Job {jobId} has been performed", context.BackgroundJob.Id);
        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
            {
                Logger.Error(
                    "Job {0} has been failed due to an exception {1}",
                    context.BackgroundJob.Id,
                    failedState.Exception);
            }
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            Logger.Debug(
                "Job {jobId} state was changed from {oldState} to {newState}",
                context.BackgroundJob.Id,
                context.OldStateName,
                context.NewState.Name);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            Logger.Debug(
                "Job {jobId} state {oldState} was unapplied.",
                context.BackgroundJob.Id,
                context.OldStateName);
        }
    }
}
