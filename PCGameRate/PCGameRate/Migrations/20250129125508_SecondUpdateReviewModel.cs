using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCGameRate.Migrations
{
    /// <inheritdoc />
    public partial class SecondUpdateReviewModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatePosted",
                table: "Reviews",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatePosted",
                table: "Reviews");
        }
    }
}
