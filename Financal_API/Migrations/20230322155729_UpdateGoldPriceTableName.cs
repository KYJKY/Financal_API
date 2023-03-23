using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Financal_API.Migrations
{
    public partial class UpdateGoldPriceTableName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "GoldPrice");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GoldPrice",
                table: "GoldPrice",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GoldPrice",
                table: "GoldPrice");

            migrationBuilder.RenameTable(
                name: "GoldPrice",
                newName: "Categories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");
        }
    }
}
