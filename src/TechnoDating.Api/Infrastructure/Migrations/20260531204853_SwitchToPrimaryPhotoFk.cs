using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnoDating.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SwitchToPrimaryPhotoFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Photos_UserId_IsPrimary",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "Photos");

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryPhotoId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PrimaryPhotoId",
                table: "AspNetUsers",
                column: "PrimaryPhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Photos_PrimaryPhotoId",
                table: "AspNetUsers",
                column: "PrimaryPhotoId",
                principalTable: "Photos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Photos_PrimaryPhotoId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PrimaryPhotoId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PrimaryPhotoId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "Photos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_UserId_IsPrimary",
                table: "Photos",
                columns: new[] { "UserId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true");
        }
    }
}
