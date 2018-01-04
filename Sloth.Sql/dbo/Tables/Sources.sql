CREATE TABLE [dbo].[Sources]
(
	[Id]           NVARCHAR(36)  NOT NULL,
	[Name]         NVARCHAR(50)  NOT NULL, 
    [Type]         NVARCHAR(50)  NOT NULL, 
    [Description]  NVARCHAR(255) NULL, 
    [OriginCode]   NVARCHAR(2)   NOT NULL, 
    [DocumentType] NVARCHAR(4)   NOT NULL, 
    [TeamId]       NVARCHAR(36)  NOT NULL, 
    CONSTRAINT [PK_Sources] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Sources_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id]),
)
