using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedifyNow.Migrations
{
    /// <inheritdoc />
    public partial class AtualizacaoModelAgendamento27052025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Atendido",
                table: "Agendamentos",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmacaoComparecimento",
                table: "Agendamentos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Atendido",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "ConfirmacaoComparecimento",
                table: "Agendamentos");
        }
    }
}
