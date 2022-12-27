using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    public partial class fixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RandomMoves",
                table: "CheckersOptions");

            migrationBuilder.DropColumn(
                name: "WhiteStarts",
                table: "CheckersOptions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RandomMoves",
                table: "CheckersOptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "WhiteStarts",
                table: "CheckersOptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
