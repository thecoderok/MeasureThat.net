CREATE TABLE [dbo].[BenchmarkTest]
(
	[Id] BIGINT IDENTITY NOT NULL PRIMARY KEY, 
    [BenchmarkId] BIGINT NOT NULL, 
    [BenchmarkText] NVARCHAR(MAX) NOT NULL, 
    [TestName] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [FK_BenchmarkTest_ToBenchmark] FOREIGN KEY ([BenchmarkId]) REFERENCES [Benchmark]([Id]) ON DELETE CASCADE
)
