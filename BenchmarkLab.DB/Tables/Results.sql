CREATE TABLE [dbo].[Result]
(
	[Id] BIGINT NOT NULL PRIMARY KEY, 
    [BenchmarkId] BIGINT NOT NULL, 
    [RawUAString] NVARCHAR(100) NOT NULL, 
    [Browser] NVARCHAR(50) NOT NULL, 
    [Created] DATETIME2 NOT NULL DEFAULT getdate(), 
    [UserId] NVARCHAR(450) NULL, 
    [DevicePlatform] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [FK_Results_ToBenchmark] FOREIGN KEY ([BenchmarkId]) REFERENCES [Benchmark]([Id]), 
    CONSTRAINT [FK_Result_ToUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id])
)
