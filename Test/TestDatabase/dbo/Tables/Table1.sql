CREATE TABLE [dbo].[Table1]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [SomeString] nvarchar(50) NULL,
    [SomeDate] date NULL,
    CONSTRAINT [PK_Table1] PRIMARY KEY CLUSTERED ([Id] ASC)
);
