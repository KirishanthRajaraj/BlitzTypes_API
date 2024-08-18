using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlitzTypes_API.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreUserProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "refreshTokenExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "typingTime",
                table: "AspNetUsers",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "refreshTokenExpiry",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "typingTime",
                table: "AspNetUsers");
        }
    }
}
