CREATE TABLE [dbo].[BenchmarkTest]
(
	[Id] INT IDENTITY NOT NULL PRIMARY KEY, 
    [BenchmarkId] INT NOT NULL, 
    [BenchmarkText] NVARCHAR(MAX) NOT NULL, 
    [TestName] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [FK_BenchmarkTest_ToBenchmark] FOREIGN KEY ([BenchmarkId]) REFERENCES [Benchmark]([Id])
)
