using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GapsiMVC.Migrations
{
    /// <inheritdoc />
    public partial class HorariosGrupos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HorariosGrupos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrupoId = table.Column<int>(type: "int", nullable: false),
                    DiaDaSemana = table.Column<int>(type: "int", nullable: false),
                    Hora = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosGrupos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosGrupos_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HorariosGrupos_GrupoId_DiaDaSemana_Hora",
                table: "HorariosGrupos",
                columns: new[] { "GrupoId", "DiaDaSemana", "Hora" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorariosGrupos");
        }
    }
}
