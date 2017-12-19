CREATE TABLE [dbo].[ApiKeys] (
    [Id]      NVARCHAR (36) NOT NULL,
    [Key]     NVARCHAR (36) NOT NULL,
    [Issued]  DATETIME2 (7) NOT NULL,
    [Revoked] DATETIME2 (7) NULL,
    [UserId]  NVARCHAR (36) NULL,
    CONSTRAINT [PK_ApiKeys] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ApiKeys_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [UK_ApiKeys_Key] UNIQUE NONCLUSTERED ([Key] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ApiKeys_Key]
    ON [dbo].[ApiKeys]([Key] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ApiKeys_UserId]
    ON [dbo].[ApiKeys]([UserId] ASC);

