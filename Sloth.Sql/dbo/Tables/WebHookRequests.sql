CREATE TABLE [dbo].[WebHookRequests]
(
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY, 
    [WebHookId] NVARCHAR(450) NOT NULL, 
    [Payload] NVARCHAR(MAX) NOT NULL, 
    [ResponseStatus] INT NULL, 
    [ResponseBody] NVARCHAR(MAX) NULL, 
    [RequestCount] INT NULL, 
    [LastRequestDate] DATETIME2 NULL, 
    [Persist] BIT NOT NULL DEFAULT 1, 
    [WebHookRequestResendJobId] NVARCHAR(450) NULL, 
    CONSTRAINT [FK_WebHookRequests_WebHooks_WebHookId] FOREIGN KEY ([WebHookId]) REFERENCES [WebHooks]([Id]), 
    CONSTRAINT [FK_WebHookRequests_WebHookRequestResendJobRecords_WebHookRequestResendJobId] FOREIGN KEY ([WebHookRequestResendJobId]) REFERENCES [WebHookRequestResendJobRecords]([Id])
)

GO

CREATE INDEX [IX_WebHookRequests_WebHookId] ON [dbo].[WebHookRequests] ([WebHookId])

GO

CREATE INDEX [IX_WebHookRequests_ResponseStatus] ON [dbo].[WebHookRequests] ([ResponseStatus])

GO

CREATE INDEX [IX_WebHookRequests_LastRequestDate] ON [dbo].[WebHookRequests] ([LastRequestDate])

GO

CREATE INDEX [IX_WebHookRequests_WebHookRequestResendJobId] ON [dbo].[WebHookRequests] ([WebHookRequestResendJobId])
