using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace BenchmarkLab.Migrations
{
    public partial class SaveThatBlob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BenchmarkTest_ToBenchmark",
                table: "BenchmarkTest");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_ToBenchmark",
                table: "Result");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultRow_ToResult",
                table: "ResultRow");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Benchmark",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SaveThatBlob",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Blob = table.Column<string>(nullable: false),
                    Language = table.Column<string>(maxLength: 40, nullable: true),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    OwnerId = table.Column<string>(maxLength: 450, nullable: false),
                    WhenCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaveThatBlob", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BenchmarkTest_ToBenchmark",
                table: "BenchmarkTest",
                column: "BenchmarkId",
                principalTable: "Benchmark",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_ToBenchmark",
                table: "Result",
                column: "BenchmarkId",
                principalTable: "Benchmark",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultRow_ToResult",
                table: "ResultRow",
                column: "ResultId",
                principalTable: "Result",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_BenchmarkTest_ToBenchmark",
                table: "BenchmarkTest");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_ToBenchmark",
                table: "Result");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultRow_ToResult",
                table: "ResultRow");

            migrationBuilder.DropTable(
                name: "SaveThatBlob");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Benchmark",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");

            migrationBuilder.AddForeignKey(
                name: "FK_BenchmarkTest_ToBenchmark",
                table: "BenchmarkTest",
                column: "BenchmarkId",
                principalTable: "Benchmark",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_ToBenchmark",
                table: "Result",
                column: "BenchmarkId",
                principalTable: "Benchmark",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultRow_ToResult",
                table: "ResultRow",
                column: "ResultId",
                principalTable: "Result",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
