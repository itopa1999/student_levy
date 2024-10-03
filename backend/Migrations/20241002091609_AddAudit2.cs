using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAudit2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4ea1f451-9807-469c-9ffb-d10777787d4a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "669d49e3-fc85-4ec8-b4f5-3c501ef2acd8");

            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "Audits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "d2deb5c4-1a6f-4c0d-a8ec-28360e258def", null, "Admin", "ADMIN" },
                    { "e1ddb9e6-8d52-41ac-b029-7dc1ed558647", null, "Student", "STUDENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d2deb5c4-1a6f-4c0d-a8ec-28360e258def");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e1ddb9e6-8d52-41ac-b029-7dc1ed558647");

            migrationBuilder.DropColumn(
                name: "User",
                table: "Audits");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4ea1f451-9807-469c-9ffb-d10777787d4a", null, "Student", "STUDENT" },
                    { "669d49e3-fc85-4ec8-b4f5-3c501ef2acd8", null, "Admin", "ADMIN" }
                });
        }
    }
}
