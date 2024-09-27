using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class updateTransactionTranId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6ae5123b-ed5c-41f4-8b40-d46b60a93a1c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ea5b7dc3-3956-4e57-a8cb-88b93853a7ec");

            migrationBuilder.AddColumn<string>(
                name: "TransID",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2398deec-62e5-4707-b0d7-75a64b38120e", null, "Admin", "ADMIN" },
                    { "df647fcd-63ea-4e4c-8f62-95c226d3f7bc", null, "Student", "STUDENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2398deec-62e5-4707-b0d7-75a64b38120e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "df647fcd-63ea-4e4c-8f62-95c226d3f7bc");

            migrationBuilder.DropColumn(
                name: "TransID",
                table: "Transaction");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "6ae5123b-ed5c-41f4-8b40-d46b60a93a1c", null, "Admin", "ADMIN" },
                    { "ea5b7dc3-3956-4e57-a8cb-88b93853a7ec", null, "Student", "STUDENT" }
                });
        }
    }
}
