CREATE TABLE [dbo].[Scrubbers] (
    [Id]                  NVARCHAR (36)  NOT NULL,
    [BatchDate]           DATETIME2 (7)  NOT NULL,
    [BatchSequenceNumber] INT            NOT NULL,
    [Chart]               NVARCHAR (1)   NOT NULL,
    [OrganizationCode]    NVARCHAR (4)   NOT NULL,
	[OriginCode]          NVARCHAR (2)   NOT NULL,
    [DocumentType]        NVARCHAR (4)   NOT NULL, 
    [Uri]                 NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_Scrubbers] PRIMARY KEY CLUSTERED ([Id] ASC)
);

