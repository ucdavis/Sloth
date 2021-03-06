CREATE TABLE [dbo].[Transactions] (
    [Id]                      NVARCHAR (450) NOT NULL,
	[Description]             NVARCHAR (MAX) NULL,
    [CreatorId]               NVARCHAR (450) NULL,
    [DocumentNumber]          NVARCHAR (14)  NULL,
    [KfsTrackingNumber]       NVARCHAR (10)  NULL,
    [MerchantTrackingNumber]  NVARCHAR (MAX) NULL,
    [MerchantTrackingUrl]     NVARCHAR (MAX) NULL,
    [ProcessorTrackingNumber] NVARCHAR (MAX) NULL,
    [ScrubberId]              NVARCHAR (450) NULL,
    [SourceId]                NVARCHAR (450) NOT NULL,
    [Status]                  NVARCHAR (MAX) NULL,
    [TransactionDate]         DATETIME2 (7)  NOT NULL,
    [ReversalOfTransactionId] NVARCHAR (450) NULL,
    [ReversalTransactionId]   NVARCHAR (450) NULL, 
    [CybersourceBankReconcileJobRecordId] NVARCHAR(450) NULL, 
    [KfsScrubberUploadJobRecordId] NVARCHAR(450) NULL, 
    CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Transactions_AspNetUsers_CreatorId] FOREIGN KEY ([CreatorId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_Transactions_Scrubbers_ScrubberId] FOREIGN KEY ([ScrubberId]) REFERENCES [dbo].[Scrubbers] ([Id]),
    CONSTRAINT [FK_Transactions_Sources_SourceId] FOREIGN KEY ([SourceId]) REFERENCES [dbo].[Sources] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Transactions_Transactions_ReversalOfTransactionId] FOREIGN KEY ([ReversalOfTransactionId]) REFERENCES [dbo].[Transactions] ([Id]),
    CONSTRAINT [FK_Transactions_Transactions_ReversalTransactionId] FOREIGN KEY ([ReversalTransactionId]) REFERENCES [dbo].[Transactions] ([Id]), 
    CONSTRAINT [FK_Transactions_CybersourceBankReconcileJobRecords_CybersourceBankReconcileJobRecordId] FOREIGN KEY ([CybersourceBankReconcileJobRecordId]) REFERENCES [CybersourceBankReconcileJobRecords]([Id]),
    CONSTRAINT [FK_Transactions_KfsScrubberUploadJobRecords_KfsScrubberUploadJobRecordId] FOREIGN KEY ([KfsScrubberUploadJobRecordId]) REFERENCES [KfsScrubberUploadJobRecords]([Id])
);












GO
CREATE NONCLUSTERED INDEX [IX_Transactions_ScrubberId]
    ON [dbo].[Transactions]([ScrubberId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Transactions_CreatorId]
    ON [dbo].[Transactions]([CreatorId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Transactions_SourceId]
    ON [dbo].[Transactions]([SourceId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Transactions_ReversalOfTransactionId]
    ON [dbo].[Transactions]([ReversalOfTransactionId] ASC) WHERE ([ReversalOfTransactionId] IS NOT NULL);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Transactions_ReversalTransactionId]
    ON [dbo].[Transactions]([ReversalTransactionId] ASC) WHERE ([ReversalTransactionId] IS NOT NULL);


GO

CREATE NONCLUSTERED INDEX [IX_Transactions_KfsScrubberUploadJobRecordId] ON [dbo].[Transactions] ([KfsScrubberUploadJobRecordId])


GO

CREATE NONCLUSTERED INDEX [IX_Transactions_CybersourceBankReconcileJobRecordId] ON [dbo].[Transactions] ([CybersourceBankReconcileJobRecordId])
