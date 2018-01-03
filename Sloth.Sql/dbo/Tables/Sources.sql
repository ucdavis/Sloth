CREATE TABLE [dbo].[Sources]
(
	[Id]           NVARCHAR(36)  NOT NULL PRIMARY KEY,
	[Name]         NVARCHAR(50)  NOT NULL, 
    [Type]         NVARCHAR(50)  NOT NULL, 
    [Description]  NVARCHAR(MAX) NOT NULL, 
    [OriginCode]   NVARCHAR(2)   NOT NULL, 
    [DocumentType] NVARCHAR(4)   NOT NULL, 
    CONSTRAINT [PK_Sources] PRIMARY KEY CLUSTERED ([Id] ASC),
)
