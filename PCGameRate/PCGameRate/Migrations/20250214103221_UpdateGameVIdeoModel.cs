using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCGameRate.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGameVIdeoModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameVideous_Games_GameId",
                table: "GameVideous");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameVideous",
                table: "GameVideous");

            migrationBuilder.RenameTable(
                name: "GameVideous",
                newName: "GameVideos");

            migrationBuilder.RenameIndex(
                name: "IX_GameVideous_GameId",
                table: "GameVideos",
                newName: "IX_GameVideos_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameVideos",
                table: "GameVideos",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameVideos_Games_GameId",
                table: "GameVideos",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "GameId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameVideos_Games_GameId",
                table: "GameVideos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameVideos",
                table: "GameVideos");

            migrationBuilder.RenameTable(
                name: "GameVideos",
                newName: "GameVideous");

            migrationBuilder.RenameIndex(
                name: "IX_GameVideos_GameId",
                table: "GameVideous",
                newName: "IX_GameVideous_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameVideous",
                table: "GameVideous",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameVideous_Games_GameId",
                table: "GameVideous",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "GameId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
