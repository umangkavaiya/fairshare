using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FairShare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase4ExpenseEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Expenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Expenses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IdempotencyKeys",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "int", nullable: false),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyKeys", x => x.Key);
                });

            migrationBuilder.InsertData(
                table: "ExpenseCategories",
                columns: new[] { "Id", "Icon", "Name" },
                values: new object[,]
                {
                    { 1, "utensils", "Food & Drink" },
                    { 2, "car", "Transport" },
                    { 3, "bed", "Lodging" },
                    { 4, "bolt", "Utilities" },
                    { 5, "film", "Entertainment" },
                    { 6, "tag", "Other" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyKeys_ExpiresAt",
                table: "IdempotencyKeys",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdempotencyKeys");

            migrationBuilder.DeleteData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Expenses");
        }
    }
}
