using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlitzTypes_API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRefreshTokenColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "refreshToken",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "refreshTokenHash",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "refreshTokenHash",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "refreshToken",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
