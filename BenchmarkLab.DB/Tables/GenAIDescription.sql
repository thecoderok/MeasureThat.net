﻿CREATE TABLE [dbo].[GenAIDescription]
(
	[Id] BIGINT NOT NULL PRIMARY KEY, 
    [Model] NVARCHAR(100) NOT NULL, 
    [Description] NVARCHAR(MAX) NOT NULL, 
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(), 
    [BenchmarkID] BIGINT NOT NULL,
    CONSTRAINT [FK_GenAI_to_benchmark] FOREIGN KEY ([BenchmarkID]) REFERENCES Benchmark([Id]) ON DELETE CASCADE
)
