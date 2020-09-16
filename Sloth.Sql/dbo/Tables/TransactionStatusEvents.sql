CREATE TABLE [dbo].[TransactionStatusEvents]
(
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY, 
    [TransactionId] NVARCHAR(450) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL, 
    [ValidFromDate] DATETIME2 NOT NULL, 
    [ValidToDate] DATETIME2 NULL, 
    [EventDetails] NVARCHAR(450) NOT NULL, 
    CONSTRAINT [FK_TransactionStatusEvents_Transactions] FOREIGN KEY ([TransactionId]) REFERENCES [Transactions]([Id])
)

GO

CREATE NONCLUSTERED INDEX [IX_TransactionStatusEvents_TransactionId] ON [dbo].[TransactionStatusEvents] ([TransactionId] ASC)

GO

CREATE NONCLUSTERED INDEX [IX_TransactionStatusEvents_Status] ON [dbo].[TransactionStatusEvents] ([Status] ASC)

GO

CREATE NONCLUSTERED INDEX [IX_TransactionStatusEvents_ValidFromDate] ON [dbo].[TransactionStatusEvents] ([ValidFromDate] ASC)

GO

CREATE NONCLUSTERED INDEX [IX_TransactionStatusEvents_ValidToDate] ON [dbo].[TransactionStatusEvents] ([ValidToDate] ASC)

GO

CREATE NONCLUSTERED INDEX [IX_TransactionStatusEvents_EventDetails] ON [dbo].[TransactionStatusEvents] ([EventDetails] ASC)
