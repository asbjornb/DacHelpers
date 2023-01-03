CREATE TABLE [dbo].[OldTable]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [SomeString] nvarchar(50) NULL,
    CONSTRAINT [PK_OldTable] PRIMARY KEY CLUSTERED ([Id] ASC)
);
