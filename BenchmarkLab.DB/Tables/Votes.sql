CREATE TABLE [dbo].[Votes]
(
	[Id] INT IDENTITY NOT NULL PRIMARY KEY, 
    [UserId] NVARCHAR(450) NOT NULL, 
    [BenchmarkId] BIGINT NOT NULL, 
    CONSTRAINT [FK_Votes_ToBenchmark] FOREIGN KEY ([BenchmarkId]) REFERENCES [Benchmark]([Id]), 
    CONSTRAINT [FK_Votes_ToUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id])
)
