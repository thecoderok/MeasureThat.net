using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BenchmarkLab.Migrations
{
    /// <inheritdoc />
    public partial class some_missing_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveThatBlob",
                table: "SaveThatBlob");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResultRow",
                table: "ResultRow");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Result",
                table: "Result");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BenchmarkTest",
                table: "BenchmarkTest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Benchmark",
                table: "Benchmark");

            migrationBuilder.RenameIndex(
                name: "IX_ResultRow_ResultId",
                table: "ResultRow",
                newName: "nci_wi_ResultRow_75A28A3426425AE8A43649289A9FFE54");

            migrationBuilder.RenameIndex(
                name: "IX_Result_BenchmarkId",
                table: "Result",
                newName: "nci_wi_Result_C113F5753A9A9D31C795A08C61DCAFAA");

            migrationBuilder.RenameIndex(
                name: "IX_BenchmarkTest_BenchmarkId",
                table: "BenchmarkTest",
                newName: "nci_wi_BenchmarkTest_41E00A71028C97E148C25E9CAA179FDC");

            migrationBuilder.AlterColumn<string>(
                name: "TestName",
                table: "ResultRow",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "RawUAString",
                table: "Result",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "OperatingSystem",
                table: "Result",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DevicePlatform",
                table: "Result",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Browser",
                table: "Result",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TestName",
                table: "BenchmarkTest",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "Deferred",
                table: "BenchmarkTest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "Benchmark",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Benchmark",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Benchmark",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedBenchmarks",
                table: "Benchmark",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WhenUpdated",
                table: "Benchmark",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__tmp_ms_x__3214EC07CD25AF6A",
                table: "SaveThatBlob",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__tmp_ms_x__3214EC075E196516",
                table: "ResultRow",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__tmp_ms_x__3214EC0765711E9A",
                table: "Result",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__tmp_ms_x__3214EC0763E2E196",
                table: "BenchmarkTest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__tmp_ms_x__3214EC077CE16A06",
                table: "Benchmark",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GenAIDescription",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    BenchmarkID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GenAIDes__3214EC0727A3BCB1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GenAI_to_benchmark",
                        column: x => x.BenchmarkID,
                        principalTable: "Benchmark",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "nci_wi_Result_30A4A6472C2CA4FBBBEA149583D9D7F4",
                table: "Result",
                column: "BenchmarkId");

            migrationBuilder.CreateIndex(
                name: "nci_wi_Result_91C7BF1B9E32D70D291795D2ADF5AF8B",
                table: "Result",
                column: "BenchmarkId");

            migrationBuilder.CreateIndex(
                name: "nci_msft_1_GenAIDescription_F83040212F44F8665218E15FD5BC6775",
                table: "GenAIDescription",
                column: "BenchmarkID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenAIDescription");

            migrationBuilder.DropPrimaryKey(
                name: "PK__tmp_ms_x__3214EC07CD25AF6A",
                table: "SaveThatBlob");

            migrationBuilder.DropPrimaryKey(
                name: "PK__tmp_ms_x__3214EC075E196516",
                table: "ResultRow");

            migrationBuilder.DropPrimaryKey(
                name: "PK__tmp_ms_x__3214EC0765711E9A",
                table: "Result");

            migrationBuilder.DropIndex(
                name: "nci_wi_Result_30A4A6472C2CA4FBBBEA149583D9D7F4",
                table: "Result");

            migrationBuilder.DropIndex(
                name: "nci_wi_Result_91C7BF1B9E32D70D291795D2ADF5AF8B",
                table: "Result");

            migrationBuilder.DropPrimaryKey(
                name: "PK__tmp_ms_x__3214EC0763E2E196",
                table: "BenchmarkTest");

            migrationBuilder.DropPrimaryKey(
                name: "PK__tmp_ms_x__3214EC077CE16A06",
                table: "Benchmark");

            migrationBuilder.DropColumn(
                name: "Deferred",
                table: "BenchmarkTest");

            migrationBuilder.DropColumn(
                name: "RelatedBenchmarks",
                table: "Benchmark");

            migrationBuilder.DropColumn(
                name: "WhenUpdated",
                table: "Benchmark");

            migrationBuilder.RenameIndex(
                name: "nci_wi_ResultRow_75A28A3426425AE8A43649289A9FFE54",
                table: "ResultRow",
                newName: "IX_ResultRow_ResultId");

            migrationBuilder.RenameIndex(
                name: "nci_wi_Result_C113F5753A9A9D31C795A08C61DCAFAA",
                table: "Result",
                newName: "IX_Result_BenchmarkId");

            migrationBuilder.RenameIndex(
                name: "nci_wi_BenchmarkTest_41E00A71028C97E148C25E9CAA179FDC",
                table: "BenchmarkTest",
                newName: "IX_BenchmarkTest_BenchmarkId");

            migrationBuilder.AlterColumn<string>(
                name: "TestName",
                table: "ResultRow",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "RawUAString",
                table: "Result",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3000)",
                oldMaxLength: 3000);

            migrationBuilder.AlterColumn<string>(
                name: "OperatingSystem",
                table: "Result",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DevicePlatform",
                table: "Result",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Browser",
                table: "Result",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TestName",
                table: "BenchmarkTest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "Benchmark",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Benchmark",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Benchmark",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveThatBlob",
                table: "SaveThatBlob",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResultRow",
                table: "ResultRow",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Result",
                table: "Result",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BenchmarkTest",
                table: "BenchmarkTest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Benchmark",
                table: "Benchmark",
                column: "Id");
        }
    }
}
