CREATE TABLE [dbo].[WebHookRequestResendJobRecords] (
    [Id]            NVARCHAR (450) NOT NULL,
    [Name]          NVARCHAR (MAX) NULL,
    [RanOn]         DATETIME2 (7)  NOT NULL,
    [Status]        NVARCHAR (20) NULL,
    CONSTRAINT [PK_WebHookRequestResendJobRecords] PRIMARY KEY CLUSTERED ([Id] ASC), 
);


GO

