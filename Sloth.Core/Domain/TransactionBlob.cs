using System;
using System.Collections.Generic;
using System.Text;

namespace Sloth.Core.Models
{
    public class TransactionBlob
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string IntegrationId { get; set; }

        public Integration Integration { get; set; }

        public string TransactionId { get; set; }

        public Transaction Transaction { get; set; }

        public string BlobId { get; set; }

        public Blob Blob { get; set; }

    }
}
