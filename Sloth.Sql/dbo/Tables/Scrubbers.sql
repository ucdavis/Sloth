CREATE TABLE [dbo].[Scrubbers] (
    [Id]                  NVARCHAR (450) NOT NULL,
    [BatchDate]           DATETIME2 (7)  NOT NULL,
    [BatchSequenceNumber] INT            NOT NULL,
    [Chart]               NVARCHAR (2)   NOT NULL,
    [DocumentType]        NVARCHAR (4)   NOT NULL,
    [OrganizationCode]    NVARCHAR (4)   NOT NULL,
    [OriginCode]          NVARCHAR (2)   NOT NULL,
    [Uri]                 NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Scrubbers] PRIMARY KEY CLUSTERED ([Id] ASC)
);



