﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ShowAndSellAPI.Models.Database;

namespace ShowAndSellAPI.Migrations
{
    [DbContext(typeof(SSDbContext))]
    [Migration("20170301055632_AddressMigration")]
    partial class AddressMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ShowAndSellAPI.Models.SSBookmark", b =>
                {
                    b.Property<string>("SSBookmarkId");

                    b.Property<string>("ItemId");

                    b.Property<string>("UserId");

                    b.HasKey("SSBookmarkId");

                    b.ToTable("Bookmarks");
                });

            modelBuilder.Entity("ShowAndSellAPI.Models.SSGroup", b =>
                {
                    b.Property<string>("SSGroupId");

                    b.Property<string>("Address");

                    b.Property<string>("AdminId");

                    b.Property<DateTime>("DateCreated");

                    b.Property<int>("ItemsSold");

                    b.Property<double>("Latitude");

                    b.Property<string>("LocationDetail");

                    b.Property<double>("Longitude");

                    b.Property<string>("Name");

                    b.HasKey("SSGroupId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("ShowAndSellAPI.Models.SSItem", b =>
                {
                    b.Property<string>("SSItemId");

                    b.Property<bool>("Approved");

                    b.Property<string>("Condition");

                    b.Property<string>("Description");

                    b.Property<string>("GroupId");

                    b.Property<string>("Name");

                    b.Property<string>("OwnerId");

                    b.Property<string>("Price");

                    b.Property<string>("Thumbnail");

                    b.HasKey("SSItemId");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("ShowAndSellAPI.Models.SSMessage", b =>
                {
                    b.Property<string>("SSMessageId");

                    b.Property<string>("Body");

                    b.Property<string>("DatePosted");

                    b.Property<string>("ItemId");

                    b.Property<string>("PosterId");

                    b.HasKey("SSMessageId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("ShowAndSellAPI.Models.SSUser", b =>
                {
                    b.Property<string>("SSUserId");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<string>("GroupId");

                    b.Property<string>("LastName");

                    b.Property<string>("Password");

                    b.HasKey("SSUserId");

                    b.ToTable("Users");
                });
        }
    }
}
