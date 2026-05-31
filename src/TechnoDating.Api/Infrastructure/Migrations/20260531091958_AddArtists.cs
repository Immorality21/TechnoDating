using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnoDating.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArtists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeadlineArtists",
                table: "Festivals");

            migrationBuilder.DropColumn(
                name: "TopArtists",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Genre = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FestivalHeadlineArtists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FestivalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtistId = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FestivalHeadlineArtists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FestivalHeadlineArtists_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FestivalHeadlineArtists_Festivals_FestivalId",
                        column: x => x.FestivalId,
                        principalTable: "Festivals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTopArtists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtistId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTopArtists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTopArtists_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTopArtists_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artists_Name",
                table: "Artists",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_Slug",
                table: "Artists",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FestivalHeadlineArtists_ArtistId",
                table: "FestivalHeadlineArtists",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_FestivalHeadlineArtists_FestivalId",
                table: "FestivalHeadlineArtists",
                column: "FestivalId");

            migrationBuilder.CreateIndex(
                name: "IX_FestivalHeadlineArtists_FestivalId_ArtistId",
                table: "FestivalHeadlineArtists",
                columns: new[] { "FestivalId", "ArtistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTopArtists_ArtistId",
                table: "UserTopArtists",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTopArtists_UserId",
                table: "UserTopArtists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTopArtists_UserId_ArtistId",
                table: "UserTopArtists",
                columns: new[] { "UserId", "ArtistId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FestivalHeadlineArtists");

            migrationBuilder.DropTable(
                name: "UserTopArtists");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.AddColumn<List<string>>(
                name: "HeadlineArtists",
                table: "Festivals",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "TopArtists",
                table: "AspNetUsers",
                type: "text[]",
                nullable: false);
        }
    }
}
