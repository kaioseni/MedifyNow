using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedifyNow.Migrations
{
    /// <inheritdoc />
    public partial class AddDuracaoPadraoComoTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "DuracaoPadrao",
                table: "TipoExames",
                type: "time",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.CreateTable(
                name: "Agendamento",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CPF = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoExameId = table.Column<int>(type: "int", nullable: false),
                    DataHoraExame = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agendamento", x => x.id);
                    table.ForeignKey(
                        name: "FK_Agendamento_TipoExames_TipoExameId",
                        column: x => x.TipoExameId,
                        principalTable: "TipoExames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agendamento_TipoExameId",
                table: "Agendamento",
                column: "TipoExameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agendamento");

            migrationBuilder.AlterColumn<double>(
                name: "DuracaoPadrao",
                table: "TipoExames",
                type: "float",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");
        }
    }
}
