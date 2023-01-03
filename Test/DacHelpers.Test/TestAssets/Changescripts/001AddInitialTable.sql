PRINT N'Creating table [dbo].[TestTable]...'

CREATE TABLE [dbo].[TestTable]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [SomeString] nvarchar(50) NULL,
    [SomeDate] date NULL,
    CONSTRAINT [PK_TestTable] PRIMARY KEY CLUSTERED ([Id] ASC)
);
