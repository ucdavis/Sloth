CREATE TABLE [dbo].[Integrations] (
    [Id]                NVARCHAR (450) NOT NULL,
    [DefaultAccount]    NVARCHAR (MAX) NULL,
    [MerchantId]        NVARCHAR (MAX) NULL,
    [ReportPasswordKey] NVARCHAR (MAX) NULL,
    [ReportUsername]    NVARCHAR (MAX) NULL,
    [SourceId]          NVARCHAR (450) NOT NULL,
    [TeamId]            NVARCHAR (450) NOT NULL,
    [Type]              NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_Integrations] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Integrations_Sources_SourceId] FOREIGN KEY ([SourceId]) REFERENCES [dbo].[Sources] ([Id]),
    CONSTRAINT [FK_Integrations_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_Integrations_TeamId]
    ON [dbo].[Integrations]([TeamId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Integrations_SourceId]
    ON [dbo].[Integrations]([SourceId] ASC);

