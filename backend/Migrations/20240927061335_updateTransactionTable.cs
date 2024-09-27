using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class updateTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "29c06820-a8c0-499e-b6a2-ec21faf09958");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3013c6a0-aacb-4135-899c-8642bb01d2d4");

            migrationBuilder.AddColumn<int>(
                name: "SemesterId",
                table: "Clearances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LevyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transaction_Levies_LevyId",
                        column: x => x.LevyId,
                        principalTable: "Levies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "6ae5123b-ed5c-41f4-8b40-d46b60a93a1c", null, "Admin", "ADMIN" },
                    { "ea5b7dc3-3956-4e57-a8cb-88b93853a7ec", null, "Student", "STUDENT" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clearances_SemesterId",
                table: "Clearances",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AppUserId",
                table: "Transaction",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_LevyId",
                table: "Transaction",
                column: "LevyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clearances_Semesters_SemesterId",
                table: "Clearances",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clearances_Semesters_SemesterId",
                table: "Clearances");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Clearances_SemesterId",
                table: "Clearances");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6ae5123b-ed5c-41f4-8b40-d46b60a93a1c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ea5b7dc3-3956-4e57-a8cb-88b93853a7ec");

            migrationBuilder.DropColumn(
                name: "SemesterId",
                table: "Clearances");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "29c06820-a8c0-499e-b6a2-ec21faf09958", null, "Student", "STUDENT" },
                    { "3013c6a0-aacb-4135-899c-8642bb01d2d4", null, "Admin", "ADMIN" }
                });
        }
    }
}
