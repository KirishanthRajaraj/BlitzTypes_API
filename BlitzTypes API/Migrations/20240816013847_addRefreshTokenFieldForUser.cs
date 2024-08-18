using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlitzTypes_API.Migrations
{
    /// <inheritdoc />
    public partial class addRefreshTokenFieldForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "refreshToken",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "refreshToken",
                table: "AspNetUsers");
        }
    }
}
