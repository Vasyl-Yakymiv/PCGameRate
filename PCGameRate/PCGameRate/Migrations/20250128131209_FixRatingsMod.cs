using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCGameRate.Migrations
{
    /// <inheritdoc />
    public partial class FixRatingsMod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_AspNetUsers_UserName",
                table: "Ratings");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Ratings",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Ratings_UserName",
                table: "Ratings",
                newName: "IX_Ratings_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_AspNetUsers_Id",
                table: "Ratings",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_AspNetUsers_Id",
                table: "Ratings");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Ratings",
                newName: "UserName");

            migrationBuilder.RenameIndex(
                name: "IX_Ratings_Id",
                table: "Ratings",
                newName: "IX_Ratings_UserName");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_AspNetUsers_UserName",
                table: "Ratings",
                column: "UserName",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
