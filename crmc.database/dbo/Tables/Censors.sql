CREATE TABLE [dbo].[Censors] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Word] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.Censors] PRIMARY KEY CLUSTERED ([Id] ASC)
);

