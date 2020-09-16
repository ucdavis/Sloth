CREATE TABLE [dbo].[Scrubbers] (
    [Id]                  NVARCHAR (450) NOT NULL,
    [BatchDate]           DATETIME2 (7)  NOT NULL,
    [BatchSequenceNumber] INT            NOT NULL,
    [SourceId]            NVARCHAR (450) NOT NULL,
    [Uri]                 NVARCHAR (MAX) NULL,
    [BlobId]              NVARCHAR(450) NULL, 
    CONSTRAINT [PK_Scrubbers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Scrubbers_Sources_SourceId] FOREIGN KEY ([SourceId]) REFERENCES [dbo].[Sources] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Scrubbers_Blobs_BlobId] FOREIGN KEY ([BlobId]) REFERENCES [Blobs]([Id])
);






GO
CREATE NONCLUSTERED INDEX [IX_Scrubbers_SourceId] ON [dbo].[Scrubbers]([SourceId] ASC);

GO

CREATE NONCLUSTERED INDEX [IX_Scrubbers_BlobId] ON [dbo].[Scrubbers] ([BlobId] ASC)
