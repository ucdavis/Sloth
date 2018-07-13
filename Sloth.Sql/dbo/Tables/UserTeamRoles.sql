CREATE TABLE [dbo].[UserTeamRoles] (
    [UserId] NVARCHAR (450) NOT NULL,
    [RoleId] NVARCHAR (450) NOT NULL,
    [TeamId] NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_UserTeamRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC, [TeamId] ASC),
    CONSTRAINT [FK_UserTeamRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserTeamRoles_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserTeamRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_UserTeamRoles_TeamId]
    ON [dbo].[UserTeamRoles]([TeamId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_UserTeamRoles_RoleId]
    ON [dbo].[UserTeamRoles]([RoleId] ASC);

