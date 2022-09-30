using System;
using System.Collections.Generic;

namespace Sloth.Core.abstractions
{
    public interface IHasTransactionIds
    {
        IEnumerable<string> GetTransactionIds();
    }
}
