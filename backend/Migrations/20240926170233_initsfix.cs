using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class initsfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c792b442-2f9a-41b1-90d5-8231bbc2ec97");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fbd4c20a-5721-4272-b187-1bfbef0c06c9");

            migrationBuilder.AlterColumn<int>(
                name: "OtpId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3d92f40f-096f-4169-bf3b-dc9910462537", null, "Student", "STUDENT" },
                    { "b66db80f-db95-4cf4-b0a5-bd99cb2fefa3", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3d92f40f-096f-4169-bf3b-dc9910462537");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b66db80f-db95-4cf4-b0a5-bd99cb2fefa3");

            migrationBuilder.AlterColumn<int>(
                name: "OtpId",
                table: "AspNetUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "c792b442-2f9a-41b1-90d5-8231bbc2ec97", null, "Admin", "ADMIN" },
                    { "fbd4c20a-5721-4272-b187-1bfbef0c06c9", null, "Student", "STUDENT" }
                });
        }
    }
}
