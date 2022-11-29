using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SongTrade.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DemoUrl",
                table: "Songs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DemoUrl",
                table: "Songs");
        }
    }
}
