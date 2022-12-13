CREATE VIEW [dbo].[View1]
AS
SELECT [Id]
    , [SomeString]
    , [SomeDate]
FROM [$(TestDatabase)].[dbo].[Table1];
