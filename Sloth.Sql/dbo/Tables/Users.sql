CREATE TABLE [dbo].[Users] (
    [Id]       NVARCHAR (450) NOT NULL,
    [Email]    NVARCHAR (MAX) NULL,
    [FullName] NVARCHAR (MAX) NULL,
    [UserName] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);





