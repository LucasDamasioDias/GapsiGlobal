using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GapsiMVC.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoEnvioLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrupoUsuarios_AspNetUsers_UsuariosId",
                table: "GrupoUsuarios");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataDaConsultaDeReferencia",
                table: "MensagensBroadcast",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GrupoDeReferenciaId",
                table: "MensagensBroadcast",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UsuarioMensagensBroadcast",
                columns: table => new
                {
                    MensagemBroadcastId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioMensagensBroadcast", x => new { x.MensagemBroadcastId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_UsuarioMensagensBroadcast_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuarioMensagensBroadcast_MensagensBroadcast_MensagemBroadcastId",
                        column: x => x.MensagemBroadcastId,
                        principalTable: "MensagensBroadcast",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MensagensBroadcast_GrupoDeReferenciaId",
                table: "MensagensBroadcast",
                column: "GrupoDeReferenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioMensagensBroadcast_UsuarioId",
                table: "UsuarioMensagensBroadcast",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoUsuarios_AspNetUsers_UsuariosId",
                table: "GrupoUsuarios",
                column: "UsuariosId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MensagensBroadcast_Grupos_GrupoDeReferenciaId",
                table: "MensagensBroadcast",
                column: "GrupoDeReferenciaId",
                principalTable: "Grupos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrupoUsuarios_AspNetUsers_UsuariosId",
                table: "GrupoUsuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagensBroadcast_Grupos_GrupoDeReferenciaId",
                table: "MensagensBroadcast");

            migrationBuilder.DropTable(
                name: "UsuarioMensagensBroadcast");

            migrationBuilder.DropIndex(
                name: "IX_MensagensBroadcast_GrupoDeReferenciaId",
                table: "MensagensBroadcast");

            migrationBuilder.DropColumn(
                name: "DataDaConsultaDeReferencia",
                table: "MensagensBroadcast");

            migrationBuilder.DropColumn(
                name: "GrupoDeReferenciaId",
                table: "MensagensBroadcast");

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoUsuarios_AspNetUsers_UsuariosId",
                table: "GrupoUsuarios",
                column: "UsuariosId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
