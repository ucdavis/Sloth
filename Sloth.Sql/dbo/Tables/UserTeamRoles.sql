CREATE TABLE [dbo].[UserTeamRoles] (
    [UserId] NVARCHAR (36) NOT NULL,
    [TeamId] NVARCHAR (36) NOT NULL,
    [RoleId] NVARCHAR (36) NOT NULL,
    CONSTRAINT [PK_UserTeams] PRIMARY KEY CLUSTERED ([UserId] ASC, [TeamId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_UserTeamRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserTeamRoles_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserTeamRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);

