CREATE TABLE [dbo].[ResultRow]
(
	[Id] BIGINT IDENTITY NOT NULL PRIMARY KEY, 
    [ResultId] BIGINT NOT NULL, 
    [ExecutionsPerSecond] REAL NOT NULL, 
    [RelativeMarginOfError] REAL NOT NULL, 
    [NumberOfSamples] INT NOT NULL, 
    [TestName] NVARCHAR(500) NOT NULL, 
    CONSTRAINT [FK_ResultRow_ToResult] FOREIGN KEY ([ResultId]) REFERENCES [Result]([Id]) ON DELETE CASCADE
)
