CREATE TABLE [dbo].[SaveThatBlob]
(
    [Id] INT NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL, 
    [OwnerId] nvarchar(450) NOT NULL,
    [WhenCreated] DATETIME2 NOT NULL , 
    [Blob] NVARCHAR(MAX) NOT NULL, 
    [Language]  NVARCHAR(40) NULL,
    CONSTRAINT [FK_SaveThatBlob_ToUsers] FOREIGN KEY (OwnerId) REFERENCES [AspNetUsers]([Id])
)
