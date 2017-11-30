CREATE TABLE [dbo].[Integrations] (
    [Id]                NVARCHAR (36)  NOT NULL,
    [TeamId]            NVARCHAR (36)  NOT NULL,
    [DefaultAccount]    NVARCHAR (MAX) NULL,
    [MerchantId]        NVARCHAR (MAX) NULL,
    [ReportPasswordKey] NVARCHAR (MAX) NULL,
    [ReportUsername]    NVARCHAR (MAX) NULL,
    [Type]              INT            NOT NULL,
    CONSTRAINT [PK_Integrations] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Integrations_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id])
);

