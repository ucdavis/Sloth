using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sloth.Api.Errors
{
    public class InternalExceptionResponse
    {
        public InternalExceptionResponse()
        {
            Success = false;
        }

        public bool Success { get; set; }
        public string CorrelationId { get; set; }
        public string Message { get; set; }
    }
}
