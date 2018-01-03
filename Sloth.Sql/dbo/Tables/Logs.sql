CREATE TABLE [dbo].[Logs] (
    [Id]              INT                IDENTITY (1, 1) NOT NULL,
    [Source]          NVARCHAR (128)     NULL,
    [Message]         NVARCHAR (MAX)     NULL,
    [MessageTemplate] NVARCHAR (MAX)     NULL,
    [Level]           NVARCHAR (128)     NULL,
    [TimeStamp]       DATETIMEOFFSET (7) NOT NULL,
    [Exception]       NVARCHAR (MAX)     NULL,
    [Properties]      XML                NULL,
    [LogEvent]        NVARCHAR (MAX)     NULL,
    [CorrelationId]   NVARCHAR (50)      NULL,
    [JobId]           NVARCHAR(50)       NULL, 
    [JobName]         NVARCHAR(50)       NULL, 
    CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Logs_Source]
    ON [dbo].[Logs]([Source] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Logs_CorrelationId]
    ON [dbo].[Logs]([CorrelationId] ASC);

