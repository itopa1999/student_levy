using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class updateTransactionTranIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_AspNetUsers_AppUserId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Levies_LevyId",
                table: "Transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2398deec-62e5-4707-b0d7-75a64b38120e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "df647fcd-63ea-4e4c-8f62-95c226d3f7bc");

            migrationBuilder.RenameTable(
                name: "Transaction",
                newName: "Transactions");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_LevyId",
                table: "Transactions",
                newName: "IX_Transactions_LevyId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_AppUserId",
                table: "Transactions",
                newName: "IX_Transactions_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0560c7fb-3932-4731-8cba-7cc53fa25e22", null, "Admin", "ADMIN" },
                    { "30559682-6379-4ce8-b266-2505e60a6d7b", null, "Student", "STUDENT" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_AppUserId",
                table: "Transactions",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Levies_LevyId",
                table: "Transactions",
                column: "LevyId",
                principalTable: "Levies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_AppUserId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Levies_LevyId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0560c7fb-3932-4731-8cba-7cc53fa25e22");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "30559682-6379-4ce8-b266-2505e60a6d7b");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "Transaction");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_LevyId",
                table: "Transaction",
                newName: "IX_Transaction_LevyId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_AppUserId",
                table: "Transaction",
                newName: "IX_Transaction_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction",
                column: "Id");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2398deec-62e5-4707-b0d7-75a64b38120e", null, "Admin", "ADMIN" },
                    { "df647fcd-63ea-4e4c-8f62-95c226d3f7bc", null, "Student", "STUDENT" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_AspNetUsers_AppUserId",
                table: "Transaction",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Levies_LevyId",
                table: "Transaction",
                column: "LevyId",
                principalTable: "Levies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
