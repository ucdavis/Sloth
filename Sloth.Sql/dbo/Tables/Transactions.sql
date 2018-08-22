CREATE TABLE [dbo].[Transactions] (
    [Id]                      NVARCHAR (450) NOT NULL,
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
    CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Transactions_AspNetUsers_CreatorId] FOREIGN KEY ([CreatorId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_Transactions_Scrubbers_ScrubberId] FOREIGN KEY ([ScrubberId]) REFERENCES [dbo].[Scrubbers] ([Id]),
    CONSTRAINT [FK_Transactions_Sources_SourceId] FOREIGN KEY ([SourceId]) REFERENCES [dbo].[Sources] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Transactions_Transactions_ReversalOfTransactionId] FOREIGN KEY ([ReversalOfTransactionId]) REFERENCES [dbo].[Transactions] ([Id])
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
CREATE NONCLUSTERED INDEX [IX_Transactions_ReversalOfTransactionId]
    ON [dbo].[Transactions]([ReversalOfTransactionId] ASC);

