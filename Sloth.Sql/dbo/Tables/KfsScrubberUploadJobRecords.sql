CREATE TABLE [dbo].[KfsScrubberUploadJobRecords] (
    [Id]            NVARCHAR (450) NOT NULL,
    [Name]          NVARCHAR (MAX) NULL,
    [RanOn]         DATETIME2 (7)  NOT NULL,
    [Status]        NVARCHAR (MAX) NULL,
    [BlobId] NVARCHAR(450) NULL, 
    CONSTRAINT [PK_KfsScrubberUploadJobRecords] PRIMARY KEY CLUSTERED ([Id] ASC), 
    CONSTRAINT [FK_KfsScrubberUploadJobRecords_Blobs] FOREIGN KEY ([BlobId]) REFERENCES [Blobs]([Id])
);


GO

CREATE INDEX [IX_KfsScrubberUploadJobRecords_BlobId] ON [dbo].[KfsScrubberUploadJobRecords] ([BlobId])
