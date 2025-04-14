IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
ALTER TABLE [BenchmarkTest] DROP CONSTRAINT [FK_BenchmarkTest_ToBenchmark];

ALTER TABLE [Result] DROP CONSTRAINT [FK_Results_ToBenchmark];

ALTER TABLE [ResultRow] DROP CONSTRAINT [FK_ResultRow_ToResult];

DROP INDEX [UserNameIndex] ON [AspNetUsers];

DROP INDEX [IX_AspNetUserRoles_UserId] ON [AspNetUserRoles];

DROP INDEX [RoleNameIndex] ON [AspNetRoles];

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benchmark]') AND [c].[name] = N'Description');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Benchmark] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Benchmark] ALTER COLUMN [Description] nvarchar(4000) NULL;

CREATE TABLE [SaveThatBlob] (
    [Id] int NOT NULL,
    [Blob] nvarchar(max) NOT NULL,
    [Language] nvarchar(40) NULL,
    [Name] nvarchar(200) NOT NULL,
    [OwnerId] nvarchar(450) NOT NULL,
    [WhenCreated] datetime2 NOT NULL,
    CONSTRAINT [PK_SaveThatBlob] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

ALTER TABLE [AspNetUserTokens] ADD CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;

ALTER TABLE [BenchmarkTest] ADD CONSTRAINT [FK_BenchmarkTest_ToBenchmark] FOREIGN KEY ([BenchmarkId]) REFERENCES [Benchmark] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Result] ADD CONSTRAINT [FK_Results_ToBenchmark] FOREIGN KEY ([BenchmarkId]) REFERENCES [Benchmark] ([Id]) ON DELETE CASCADE;

ALTER TABLE [ResultRow] ADD CONSTRAINT [FK_ResultRow_ToResult] FOREIGN KEY ([ResultId]) REFERENCES [Result] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220105060240_SaveThatBlob', N'9.0.4');

ALTER TABLE [SaveThatBlob] DROP CONSTRAINT [PK_SaveThatBlob];

ALTER TABLE [ResultRow] DROP CONSTRAINT [PK_ResultRow];

ALTER TABLE [Result] DROP CONSTRAINT [PK_Result];

ALTER TABLE [BenchmarkTest] DROP CONSTRAINT [PK_BenchmarkTest];

ALTER TABLE [Benchmark] DROP CONSTRAINT [PK_Benchmark];

EXEC sp_rename N'[ResultRow].[IX_ResultRow_ResultId]', N'nci_wi_ResultRow_75A28A3426425AE8A43649289A9FFE54', 'INDEX';

EXEC sp_rename N'[Result].[IX_Result_BenchmarkId]', N'nci_wi_Result_C113F5753A9A9D31C795A08C61DCAFAA', 'INDEX';

EXEC sp_rename N'[BenchmarkTest].[IX_BenchmarkTest_BenchmarkId]', N'nci_wi_BenchmarkTest_41E00A71028C97E148C25E9CAA179FDC', 'INDEX';

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ResultRow]') AND [c].[name] = N'TestName');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ResultRow] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [ResultRow] ALTER COLUMN [TestName] nvarchar(500) NOT NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Result]') AND [c].[name] = N'RawUAString');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Result] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Result] ALTER COLUMN [RawUAString] nvarchar(3000) NOT NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Result]') AND [c].[name] = N'OperatingSystem');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Result] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Result] ALTER COLUMN [OperatingSystem] nvarchar(500) NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Result]') AND [c].[name] = N'DevicePlatform');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Result] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Result] ALTER COLUMN [DevicePlatform] nvarchar(500) NULL;

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Result]') AND [c].[name] = N'Browser');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Result] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Result] ALTER COLUMN [Browser] nvarchar(500) NULL;

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BenchmarkTest]') AND [c].[name] = N'TestName');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [BenchmarkTest] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [BenchmarkTest] ALTER COLUMN [TestName] nvarchar(500) NOT NULL;

ALTER TABLE [BenchmarkTest] ADD [Deferred] bit NOT NULL DEFAULT CAST(0 AS bit);

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benchmark]') AND [c].[name] = N'Version');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Benchmark] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Benchmark] ADD DEFAULT 1 FOR [Version];

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benchmark]') AND [c].[name] = N'OwnerId');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Benchmark] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [Benchmark] ALTER COLUMN [OwnerId] nvarchar(450) NULL;

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benchmark]') AND [c].[name] = N'Description');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Benchmark] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Benchmark] ALTER COLUMN [Description] nvarchar(max) NULL;

ALTER TABLE [Benchmark] ADD [RelatedBenchmarks] nvarchar(500) NULL;

ALTER TABLE [Benchmark] ADD [WhenUpdated] datetime2 NULL;

ALTER TABLE [SaveThatBlob] ADD CONSTRAINT [PK__tmp_ms_x__3214EC07CD25AF6A] PRIMARY KEY ([Id]);

ALTER TABLE [ResultRow] ADD CONSTRAINT [PK__tmp_ms_x__3214EC075E196516] PRIMARY KEY ([Id]);

ALTER TABLE [Result] ADD CONSTRAINT [PK__tmp_ms_x__3214EC0765711E9A] PRIMARY KEY ([Id]);

ALTER TABLE [BenchmarkTest] ADD CONSTRAINT [PK__tmp_ms_x__3214EC0763E2E196] PRIMARY KEY ([Id]);

ALTER TABLE [Benchmark] ADD CONSTRAINT [PK__tmp_ms_x__3214EC077CE16A06] PRIMARY KEY ([Id]);

CREATE TABLE [GenAIDescription] (
    [Id] bigint NOT NULL IDENTITY,
    [Model] nvarchar(100) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [CreatedDate] datetime NOT NULL DEFAULT ((getdate())),
    [BenchmarkID] bigint NOT NULL,
    CONSTRAINT [PK__GenAIDes__3214EC0727A3BCB1] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GenAI_to_benchmark] FOREIGN KEY ([BenchmarkID]) REFERENCES [Benchmark] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [nci_wi_Result_30A4A6472C2CA4FBBBEA149583D9D7F4] ON [Result] ([BenchmarkId]);

CREATE INDEX [nci_wi_Result_91C7BF1B9E32D70D291795D2ADF5AF8B] ON [Result] ([BenchmarkId]);

CREATE INDEX [nci_msft_1_GenAIDescription_F83040212F44F8665218E15FD5BC6775] ON [GenAIDescription] ([BenchmarkID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250413193228_some_missing_migration', N'9.0.4');

COMMIT;
GO

