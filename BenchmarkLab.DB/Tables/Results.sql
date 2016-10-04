CREATE TABLE [dbo].[Result]
(
	[Id] BIGINT IDENTITY NOT NULL PRIMARY KEY, 
    [BenchmarkId] BIGINT NOT NULL, 
    [RawUAString] NVARCHAR(300) NOT NULL, 
    [Browser] NVARCHAR(100) NULL, 
    [Created] DATETIME2 NOT NULL DEFAULT getdate(), 
    [UserId] NVARCHAR(450) NULL, 
    [DevicePlatform] NVARCHAR(100) NULL, 
    [OperatingSystem] NVARCHAR(100) NULL, 
    [Version] INT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_Results_ToBenchmark] FOREIGN KEY ([BenchmarkId]) REFERENCES [Benchmark]([Id]) ON DELETE CASCADE, 
    CONSTRAINT [FK_Result_ToUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id])
)
