using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShowAndSellAPI.Migrations
{
    public partial class BookmarkMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    SSBookmarkId = table.Column<string>(nullable: false),
                    itemId = table.Column<string>(nullable: true),
                    userId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmarks", x => x.SSBookmarkId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Bookmarks");
        }
    }
}
