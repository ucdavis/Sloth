CREATE TABLE [dbo].[Users] (
    [Id]       NVARCHAR (36)  NOT NULL,
    [Email]    NVARCHAR (256) NULL,
    [UserName] NVARCHAR (256) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);

