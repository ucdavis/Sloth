CREATE TABLE [dbo].[TeamRoles] (
    [Id]   NVARCHAR (450) NOT NULL,
    [Name] NVARCHAR (450) NULL,
    CONSTRAINT [PK_TeamRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TeamRoles_Name]
    ON [dbo].[TeamRoles]([Name] ASC) WHERE ([Name] IS NOT NULL);

