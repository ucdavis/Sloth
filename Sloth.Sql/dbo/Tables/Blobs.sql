CREATE TABLE [dbo].[Blobs]
(
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY, 
    [FileName] NVARCHAR(450) NOT NULL, 
    [Uri] NVARCHAR(450) NOT NULL, 
    [Description] NVARCHAR(450) NOT NULL, 
    [Container] NVARCHAR(450) NOT NULL, 
    [MediaType] NVARCHAR(450) NOT NULL, 
    [UploadedDate] DATETIME2 NOT NULL
)

GO

CREATE INDEX [IX_Blobs_FileName] ON [dbo].[Blobs] ([FileName] ASC)

GO

CREATE INDEX [IX_Blobs_UploadedDate] ON [dbo].[Blobs] ([UploadedDate] ASC)

GO

CREATE INDEX [IX_Blobs_Uri] ON [dbo].[Blobs] ([Uri] ASC)
