using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    public partial class migration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckersGameStates_CheckersStates_CheckersStateId",
                table: "CheckersGameStates");

            migrationBuilder.DropTable(
                name: "CheckersStates");

            migrationBuilder.DropIndex(
                name: "IX_CheckersGameStates_CheckersStateId",
                table: "CheckersGameStates");

            migrationBuilder.DropColumn(
                name: "CheckersStateId",
                table: "CheckersGameStates");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CheckersStateId",
                table: "CheckersGameStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CheckersStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NextMoveByBlack = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckersStates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckersGameStates_CheckersStateId",
                table: "CheckersGameStates",
                column: "CheckersStateId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckersGameStates_CheckersStates_CheckersStateId",
                table: "CheckersGameStates",
                column: "CheckersStateId",
                principalTable: "CheckersStates",
                principalColumn: "Id");
        }
    }
}
