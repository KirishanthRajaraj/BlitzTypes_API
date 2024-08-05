using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlitzTypes_API.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomFieldsToIdentityUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "highScoreAccuracy",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "highScoreWPM",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "joinedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preferredLanguage",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preferredTime",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "secondsWritten",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "highScoreAccuracy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "highScoreWPM",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "joinedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "preferredLanguage",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "preferredTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "secondsWritten",
                table: "AspNetUsers");
        }
    }
}
