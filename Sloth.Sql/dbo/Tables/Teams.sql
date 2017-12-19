﻿CREATE TABLE [dbo].[Teams] (
    [Id]   NVARCHAR (36)  NOT NULL,
    [Name] NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_Teams] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_Teams_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);

