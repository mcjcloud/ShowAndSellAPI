using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShowAndSellAPI.Migrations
{
    public partial class latlongmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Groups");

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "Groups",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "Groups",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Groups");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Groups",
                nullable: true);
        }
    }
}
