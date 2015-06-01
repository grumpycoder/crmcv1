CREATE TABLE [dbo].[Persons] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [AccountId]       VARCHAR (50)    NULL,
    [Firstname]       VARCHAR (50)    NULL,
    [Lastname]        VARCHAR (50)    NULL,
    [EmailAddress]    VARCHAR (75)    NULL,
    [Zipcode]         VARCHAR (10)    NULL,
    [IsDonor]         BIT             CONSTRAINT [DF__Persons__IsDonor__1FCDBCEB] DEFAULT ((0)) NULL,
    [IsPriority]      BIT             CONSTRAINT [DF__Persons__IsPrior__20C1E124] DEFAULT ((0)) NULL,
    [FuzzyMatchValue] DECIMAL (18, 2) NULL,
    [DateCreated]     DATETIME2 (7)   CONSTRAINT [DF__Persons__DateCre__21B6055D] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_dbo.Persons] PRIMARY KEY CLUSTERED ([Id] ASC)
);



