using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sloth.Core.Jobs
{
    public interface IJobServiceBase
    {
        Task Execute(CancellationToken cancellationToken);
    }
}
