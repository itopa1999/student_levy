using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class retrying : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0560c7fb-3932-4731-8cba-7cc53fa25e22");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "30559682-6379-4ce8-b266-2505e60a6d7b");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4c80ebf8-5337-4d09-a413-b3ce349ae038", null, "Admin", "ADMIN" },
                    { "c8678431-ece6-4b44-bc9f-243f3da93639", null, "Student", "STUDENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4c80ebf8-5337-4d09-a413-b3ce349ae038");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c8678431-ece6-4b44-bc9f-243f3da93639");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0560c7fb-3932-4731-8cba-7cc53fa25e22", null, "Admin", "ADMIN" },
                    { "30559682-6379-4ce8-b266-2505e60a6d7b", null, "Student", "STUDENT" }
                });
        }
    }
}
