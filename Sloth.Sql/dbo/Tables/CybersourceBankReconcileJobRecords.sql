CREATE TABLE [dbo].[CybersourceBankReconcileJobRecords] (
    [Id]            NVARCHAR (450) NOT NULL,
    [Name]          NVARCHAR (MAX) NULL,
    [ProcessedDate] DATETIME2 (7)  NOT NULL,
    [RanOn]         DATETIME2 (7)  NOT NULL,
    [Status]        NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_CybersourceBankReconcileJobRecords] PRIMARY KEY CLUSTERED ([Id] ASC)
);

