﻿CREATE TABLE [dbo].[Teams] (
    [Id]   NVARCHAR (450) NOT NULL,
    [Name] NVARCHAR (450) NULL,
    CONSTRAINT [PK_Teams] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Teams_Name]
    ON [dbo].[Teams]([Name] ASC) WHERE ([Name] IS NOT NULL);

