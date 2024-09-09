using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlitzTypes_API.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreUserProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "highScoreWPM",
                table: "AspNetUsers",
                newName: "testAmount");

            migrationBuilder.AddColumn<string>(
                name: "BlitztypesTitle",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "highScoreWPM_15_sec",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "highScoreWPM_30_sec",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "highScoreWPM_60_sec",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlitztypesTitle",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "highScoreWPM_15_sec",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "highScoreWPM_30_sec",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "highScoreWPM_60_sec",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "testAmount",
                table: "AspNetUsers",
                newName: "highScoreWPM");
        }
    }
}
