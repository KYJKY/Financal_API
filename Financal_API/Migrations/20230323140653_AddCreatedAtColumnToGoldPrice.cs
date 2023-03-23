using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Financal_API.Migrations
{
    public partial class AddCreatedAtColumnToGoldPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "GoldPrice",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getdate()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "GoldPrice");
        }
    }
}
