using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShowAndSellAPI.Migrations
{
    public partial class doublelatlongmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Groups",
                nullable: false);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Groups",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Longitude",
                table: "Groups",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Latitude",
                table: "Groups",
                nullable: true);
        }
    }
}
