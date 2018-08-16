CREATE TABLE [dbo].[Transfers] (
    [Id]             NVARCHAR (450)  NOT NULL,
    [Account]        NVARCHAR (7)    NOT NULL,
    [Amount]         DECIMAL (18, 2) NOT NULL,
    [Chart]          NVARCHAR (1)    NOT NULL,
    [Description]    NVARCHAR (40)   NOT NULL,
    [Direction]      INT             NOT NULL,
    [FiscalPeriod]   INT             NULL,
    [FiscalYear]     INT             NULL,
    [ObjectCode]     NVARCHAR (4)    NOT NULL,
    [ObjectType]     NVARCHAR (2)    NULL,
    [Project]        NVARCHAR (10)   NULL,
    [ReferenceId]    NVARCHAR (8)    NULL,
    [SequenceNumber] INT             NULL,
    [SubAccount]     NVARCHAR (5)    NULL,
    [SubObjectCode]  NVARCHAR (3)    NULL,
    [TransactionId]  NVARCHAR (450)  NULL,
    CONSTRAINT [PK_Transfers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Transfers_Transactions_TransactionId] FOREIGN KEY ([TransactionId]) REFERENCES [dbo].[Transactions] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_Transfers_TransactionId]
    ON [dbo].[Transfers]([TransactionId] ASC);

