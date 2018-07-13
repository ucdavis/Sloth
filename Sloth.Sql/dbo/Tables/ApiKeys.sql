CREATE TABLE [dbo].[ApiKeys] (
    [Id]      NVARCHAR (450) NOT NULL,
    [Issued]  DATETIME2 (7)  NOT NULL,
    [Key]     NVARCHAR (450) NULL,
    [Revoked] DATETIME2 (7)  NULL,
    [TeamId]  NVARCHAR (450) NULL,
    CONSTRAINT [PK_ApiKeys] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ApiKeys_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ApiKeys_Key]
    ON [dbo].[ApiKeys]([Key] ASC) WHERE ([Key] IS NOT NULL);




GO
CREATE NONCLUSTERED INDEX [IX_ApiKeys_TeamId]
    ON [dbo].[ApiKeys]([TeamId] ASC);

