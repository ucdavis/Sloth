CREATE TABLE [dbo].[KfsScrubberUploadJobRecords] (
    [Id]            NVARCHAR (450) NOT NULL,
    [Name]          NVARCHAR (MAX) NULL,
    [RanOn]         DATETIME2 (7)  NOT NULL,
    [Status]        NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_KfsScrubberUploadJobRecords] PRIMARY KEY CLUSTERED ([Id] ASC), 
);


GO

