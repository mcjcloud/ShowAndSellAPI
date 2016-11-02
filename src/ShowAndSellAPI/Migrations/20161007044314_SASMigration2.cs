using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShowAndSellAPI.Migrations
{
    public partial class SASMigration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Items",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Price",
                table: "Items",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Items");

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "Items",
                nullable: false);
        }
    }
}
