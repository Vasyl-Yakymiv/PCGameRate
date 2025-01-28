using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCGameRate.Migrations
{
    /// <inheritdoc />
    public partial class FixRatingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_AspNetUsers_UserId",
                table: "Ratings");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Ratings",
                newName: "UserName");

            migrationBuilder.RenameIndex(
                name: "IX_Ratings_UserId",
                table: "Ratings",
                newName: "IX_Ratings_UserName");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_AspNetUsers_UserName",
                table: "Ratings",
                column: "UserName",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_AspNetUsers_UserName",
                table: "Ratings");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Ratings",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Ratings_UserName",
                table: "Ratings",
                newName: "IX_Ratings_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_AspNetUsers_UserId",
                table: "Ratings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
