using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GapsiMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelasDeMensagensBroadcast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GrupoUsuarios",
                table: "GrupoUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_GrupoUsuarios_UsuariosId",
                table: "GrupoUsuarios");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GrupoUsuarios",
                table: "GrupoUsuarios",
                columns: new[] { "UsuariosId", "GruposId" });

            migrationBuilder.CreateTable(
                name: "MensagensBroadcast",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Conteudo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnviadoPorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagensBroadcast", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MensagensBroadcast_AspNetUsers_EnviadoPorUserId",
                        column: x => x.EnviadoPorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoMensagensBroadcast",
                columns: table => new
                {
                    MensagemBroadcastId = table.Column<int>(type: "int", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoMensagensBroadcast", x => new { x.MensagemBroadcastId, x.GrupoId });
                    table.ForeignKey(
                        name: "FK_GrupoMensagensBroadcast_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrupoMensagensBroadcast_MensagensBroadcast_MensagemBroadcastId",
                        column: x => x.MensagemBroadcastId,
                        principalTable: "MensagensBroadcast",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrupoUsuarios_GruposId",
                table: "GrupoUsuarios",
                column: "GruposId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoMensagensBroadcast_GrupoId",
                table: "GrupoMensagensBroadcast",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensBroadcast_EnviadoPorUserId",
                table: "MensagensBroadcast",
                column: "EnviadoPorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrupoMensagensBroadcast");

            migrationBuilder.DropTable(
                name: "MensagensBroadcast");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GrupoUsuarios",
                table: "GrupoUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_GrupoUsuarios_GruposId",
                table: "GrupoUsuarios");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GrupoUsuarios",
                table: "GrupoUsuarios",
                columns: new[] { "GruposId", "UsuariosId" });

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinatarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsSpam = table.Column<bool>(type: "bit", nullable: false),
                    RemetenteId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensagens_AspNetUsers_DestinatarioId",
                        column: x => x.DestinatarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mensagens_AspNetUsers_RemetenteId",
                        column: x => x.RemetenteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrupoUsuarios_UsuariosId",
                table: "GrupoUsuarios",
                column: "UsuariosId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_DestinatarioId",
                table: "Mensagens",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_RemetenteId",
                table: "Mensagens",
                column: "RemetenteId");
        }
    }
}
