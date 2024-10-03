using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAudit22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Audits_AspNetUsers_AppUserId",
                table: "Audits");

            migrationBuilder.DropIndex(
                name: "IX_Audits_AppUserId",
                table: "Audits");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5179702a-0461-4341-97db-0b4b42025d8b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a9af3916-64d5-4339-921e-6f1822af061a");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Audits");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "Audits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4ea1f451-9807-469c-9ffb-d10777787d4a", null, "Student", "STUDENT" },
                    { "669d49e3-fc85-4ec8-b4f5-3c501ef2acd8", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4ea1f451-9807-469c-9ffb-d10777787d4a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "669d49e3-fc85-4ec8-b4f5-3c501ef2acd8");

            migrationBuilder.AlterColumn<bool>(
                name: "Action",
                table: "Audits",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Audits",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5179702a-0461-4341-97db-0b4b42025d8b", null, "Student", "STUDENT" },
                    { "a9af3916-64d5-4339-921e-6f1822af061a", null, "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Audits_AppUserId",
                table: "Audits",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Audits_AspNetUsers_AppUserId",
                table: "Audits",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
