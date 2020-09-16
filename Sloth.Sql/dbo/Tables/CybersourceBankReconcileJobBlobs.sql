CREATE TABLE [dbo].[CybersourceBankReconcileJobBlobs]
(
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY, 
    [CybersourceBankReconcileJobRecordId] NVARCHAR(450) NOT NULL, 
    [IntegrationId] NVARCHAR(450) NOT NULL, 
    [BlobId] NVARCHAR(450) NOT NULL, 
    CONSTRAINT [FK_CybersourceBankReconcileJobBlobs_CybersourceBankReconcileJobRecords] FOREIGN KEY ([CybersourceBankReconcileJobRecordId]) REFERENCES [CybersourceBankReconcileJobRecords]([Id]), 
    CONSTRAINT [FK_CybersourceBankReconcileJobBlobs_Integrations] FOREIGN KEY ([IntegrationId]) REFERENCES [Integrations]([Id]), 
    CONSTRAINT [FK_CybersourceBankReconcileJobBlobs_Blobs] FOREIGN KEY ([BlobId]) REFERENCES [Blobs]([Id])
)

GO

CREATE INDEX [IX_CybersourceBankReconcileJobBlobs_CybersourceBankReconcileJobRecordId] ON [dbo].[CybersourceBankReconcileJobBlobs] ([CybersourceBankReconcileJobRecordId])

GO

CREATE INDEX [IX_CybersourceBankReconcileJobBlobs_IntegrationId] ON [dbo].[CybersourceBankReconcileJobBlobs] ([IntegrationId])

GO

CREATE INDEX [IX_CybersourceBankReconcileJobBlobs_BlobId] ON [dbo].[CybersourceBankReconcileJobBlobs] ([BlobId])
