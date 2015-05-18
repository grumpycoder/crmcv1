CREATE TABLE [dbo].[AppConfigs] (
    [Id]                 INT          IDENTITY (1, 1) NOT NULL,
    [HubName]            VARCHAR (50) NULL,
    [WebServerURL]       VARCHAR (50) NULL,
    [Volume]             FLOAT (53)   NULL,
    [ScrollSpeed]        INT          NULL,
    [AddNewItemSpeed]    FLOAT (53)   NULL,
    [MinFontSize]        INT          NULL,
    [MaxFontSize]        INT          NULL,
    [MinFontSizeVIP]     INT          NULL,
    [MaxFontSizeVIP]     INT          NULL,
    [FontFamily]         VARCHAR (50) NULL,
    [AudioFilePath]      VARCHAR (50) NULL,
    [UseLocalDataSource] BIT          NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

