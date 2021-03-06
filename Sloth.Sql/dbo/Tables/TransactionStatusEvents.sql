CREATE TABLE [dbo].[TransactionStatusEvents]
(
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY, 
    [TransactionId] NVARCHAR(450) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL, 
    [EventDate] DATETIME2 NOT NULL, 
    [EventDetails] NVARCHAR(450) NOT NULL, 
    CONSTRAINT [FK_TransactionStatusEvents_Transactions] FOREIGN KEY ([TransactionId]) REFERENCES [Transactions]([Id])
)

GO

CREATE NONCLUSTERED INDEX [IX_TransactionStatusEvents_TransactionId] ON [dbo].[TransactionStatusEvents] ([TransactionId] ASC)

GO

CREATE NONCLUSTERED INDEX [IX_TransactionStatusEvents_Status] ON [dbo].[TransactionStatusEvents] ([Status] ASC)

GO

CREATE NONCLUSTERED INDEX [IX_TransactionStatusEvents_EventDate] ON [dbo].[TransactionStatusEvents] ([EventDate] ASC)

GO
