CREATE TABLE [dbo].[Benchmark]
(
	[Id] BIGINT IDENTITY NOT NULL PRIMARY KEY, 
    [Name] NVARCHAR(200) NOT NULL, 
    [Description] NVARCHAR(MAX) NULL, 
    [OwnerId] nvarchar(450) NULL, 
    [WhenCreated] DATETIME2 NOT NULL , 
    [ScriptPreparationCode] NVARCHAR(MAX) NULL, 
    [HtmlPreparationCode] NVARCHAR(MAX) NULL, 
    [Version] INT NOT NULL DEFAULT 1, 
    [RelatedBenchmarks] NVARCHAR(500) NULL, 
    [WhenUpdated] DATETIME2 NULL, 
    CONSTRAINT [FK_Benchmark_ToUsers] FOREIGN KEY (OwnerId) REFERENCES [AspNetUsers]([Id])
)

GO

CREATE INDEX [IX_Benchmark_WhenCreated] ON [dbo].[Benchmark] (WhenCreated)
