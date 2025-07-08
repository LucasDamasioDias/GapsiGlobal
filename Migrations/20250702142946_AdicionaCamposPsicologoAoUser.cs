using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GapsiMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCamposPsicologoAoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleRefreshToken",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Biografia",
                table: "AspNetUsers",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CRP",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Biografia",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CRP",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "GoogleRefreshToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
