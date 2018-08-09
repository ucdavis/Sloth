CREATE TABLE [dbo].[Sources] (
    [Id]                    NVARCHAR (450) NOT NULL,
    [Chart]                 NVARCHAR (2)   NOT NULL,
    [Description]           NVARCHAR (255) NULL,
    [DocumentType]          NVARCHAR (4)   NOT NULL,
    [KfsFtpPasswordKeyName] NVARCHAR (MAX) NULL,
    [KfsFtpUsername]        NVARCHAR (MAX) NULL,
    [Name]                  NVARCHAR (50)  NOT NULL,
    [OrganizationCode]      NVARCHAR (4)   NOT NULL,
    [OriginCode]            NVARCHAR (2)   NOT NULL,
    [TeamId]                NVARCHAR (450) NOT NULL,
    [Type]                  NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_Sources] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Sources_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id])
);





GO
CREATE NONCLUSTERED INDEX [IX_Sources_TeamId]
    ON [dbo].[Sources]([TeamId] ASC);

