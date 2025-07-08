using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GapsiMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaIsDerivadoENomeBaseAosGrupos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_Grupos_GrupoId",
                table: "Consultas");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Grupos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDerivado",
                table: "Grupos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NomeBase",
                table: "Grupos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

           
            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_Grupos_GrupoId",
                table: "Consultas",
                column: "GrupoId",
                principalTable: "Grupos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_Grupos_GrupoId",
                table: "Consultas");

            migrationBuilder.DropTable(
                name: "ComprovantesPagamento");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "ImagemUrl",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "IsDerivado",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "NomeBase",
                table: "Grupos");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Grupos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_Grupos_GrupoId",
                table: "Consultas",
                column: "GrupoId",
                principalTable: "Grupos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
