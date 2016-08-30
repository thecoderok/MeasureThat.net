using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MeasureThat.Net.Migrations
{
    public partial class CurrentMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Benchmark",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "nchar(400)", nullable: true),
                    Name = table.Column<string>(nullable: false),
                    OwnerId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benchmark", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BenchmarkVersion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BenchmarkId = table.Column<int>(nullable: false),
                    HtmlPreparationCode = table.Column<string>(nullable: true),
                    ScriptPreparationCode = table.Column<string>(nullable: true),
                    Version = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenchmarkVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BenchmarkVersion_ToBenchmark",
                        column: x => x.BenchmarkId,
                        principalTable: "Benchmark",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BenchmarkTest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BenchmarkText = table.Column<string>(nullable: false),
                    BenchmarkVersionId = table.Column<int>(nullable: false),
                    WhenCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenchmarkTest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BenchmarkTest_ToBenchmarkVersion",
                        column: x => x.BenchmarkVersionId,
                        principalTable: "BenchmarkVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenchmarkTest_BenchmarkVersionId",
                table: "BenchmarkTest",
                column: "BenchmarkVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_BenchmarkVersion_BenchmarkId",
                table: "BenchmarkVersion",
                column: "BenchmarkId");

            migrationBuilder.CreateIndex(
                name: "IX_BenchmarkVersion_Unique",
                table: "BenchmarkVersion",
                columns: new[] { "BenchmarkId", "Version" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenchmarkTest");

            migrationBuilder.DropTable(
                name: "BenchmarkVersion");

            migrationBuilder.DropTable(
                name: "Benchmark");
        }
    }
}
