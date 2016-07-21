CREATE TABLE [dbo].[BenchmarkTest]
(
	[Id] INT IDENTITY NOT NULL PRIMARY KEY, 
    [BenchmarkVersionId] INT NOT NULL, 
    [BenchmarkText] NVARCHAR(MAX) NOT NULL, 
    [WhenCreated] DATETIME2 NOT NULL DEFAULT GETDATE(), 
    [TestName] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [FK_BenchmarkTest_ToBenchmarkVersion] FOREIGN KEY ([BenchmarkVersionId]) REFERENCES [BenchmarkVersion]([Id])
)
