CREATE TABLE [dbo].[BenchmarkVersion]
(
	[Id] INT IDENTITY NOT NULL PRIMARY KEY, 
    [BenchmarkVersion] TINYINT NOT NULL, 
    [BenchmarkId] INT NOT NULL, 
    [ScriptPreparationCode] NVARCHAR(MAX) NULL, 
    [HtmlPreparationCode] NVARCHAR(MAX) NULL, 
    CONSTRAINT [FK_BenchmarkVersion_ToBenchmark] FOREIGN KEY ([BenchmarkId]) REFERENCES [Benchmark]([Id])
)

GO

CREATE INDEX [IX_BenchmarkVersion_Unique] ON [dbo].[BenchmarkVersion] (BenchmarkId, BenchmarkVersion)
