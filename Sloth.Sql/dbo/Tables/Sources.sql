CREATE TABLE [dbo].[Sources] (
    [Id]           NVARCHAR (450) NOT NULL,
    [Description]  NVARCHAR (255) NULL,
    [DocumentType] NVARCHAR (4)   NOT NULL,
    [Name]         NVARCHAR (50)  NOT NULL,
    [OriginCode]   NVARCHAR (2)   NOT NULL,
    [TeamId]       NVARCHAR (450) NOT NULL,
    [Type]         NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_Sources] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Sources_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id]) ON DELETE CASCADE
);



GO
CREATE NONCLUSTERED INDEX [IX_Sources_TeamId]
    ON [dbo].[Sources]([TeamId] ASC);

