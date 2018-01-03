CREATE TABLE [dbo].[Transactions] (
    [Id]                      NVARCHAR (36)  NOT NULL,
    [CreatorId]               NVARCHAR (36)  NULL,
    [DocumentNumber]          NVARCHAR (14)  NOT NULL,
    [KfsTrackingNumber]       NVARCHAR (10)  NOT NULL,
    [MerchantTrackingNumber]  NVARCHAR (MAX) NOT NULL,
    [OriginCode]              NVARCHAR (2)   NOT NULL,
    [DocumentType]            NVARCHAR (4)   NOT NULL, 
    [ProcessorTrackingNumber] NVARCHAR (MAX) NULL,
    [ScrubberId]              NVARCHAR (36)  NULL,
    [Status]                  NVARCHAR (36)  NOT NULL,
    [TransactionDate]         DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Transactions_Scrubbers_ScrubberId] FOREIGN KEY ([ScrubberId]) REFERENCES [dbo].[Scrubbers] ([Id]),
    CONSTRAINT [FK_Transactions_Users_CreatorId] FOREIGN KEY ([CreatorId]) REFERENCES [dbo].[Users] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Transactions_ScrubberId]
    ON [dbo].[Transactions]([ScrubberId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Transactions_CreatorId]
    ON [dbo].[Transactions]([CreatorId] ASC);

