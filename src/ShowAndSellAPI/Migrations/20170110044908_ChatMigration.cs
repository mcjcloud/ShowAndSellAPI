using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShowAndSellAPI.Migrations
{
    public partial class ChatMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Admin",
                table: "Groups");

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    SSMessageId = table.Column<string>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    DatePosted = table.Column<string>(nullable: true),
                    ItemId = table.Column<string>(nullable: true),
                    PosterId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.SSMessageId);
                });

            migrationBuilder.AddColumn<string>(
                name: "AdminId",
                table: "Groups",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Groups",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationDetail",
                table: "Groups",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Bookmarks",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "itemId",
                table: "Bookmarks",
                newName: "ItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "LocationDetail",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "Admin",
                table: "Groups",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Bookmarks",
                newName: "userId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Bookmarks",
                newName: "itemId");
        }
    }
}
