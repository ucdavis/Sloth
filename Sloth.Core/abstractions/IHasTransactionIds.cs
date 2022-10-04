using System;
using System.Collections.Generic;

namespace Sloth.Core.Abstractions
{
    public interface IHasTransactionIds
    {
        IEnumerable<string> GetTransactionIds();
    }
}
