using System;
using System.Collections.Generic;
using System.Text;
using Sloth.Core.Models;

namespace Sloth.Core.Domain
{
    public class CybersourceBankReconcileJobBlob
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string IntegrationId { get; set; }

        public Integration Integration { get; set; }

        public string CybersourceBankReconcileJobRecordId { get; set; }

        public CybersourceBankReconcileJobRecord CybersourceBankReconcileJobRecord { get; set; }

        public string BlobId { get; set; }

        public Blob Blob { get; set; }

    }
}
