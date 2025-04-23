using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedifyNow.Migrations
{
    /// <inheritdoc />
    public partial class ModelAgendamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agendamento_TipoExames_TipoExameId",
                table: "Agendamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Agendamento",
                table: "Agendamento");

            migrationBuilder.RenameTable(
                name: "Agendamento",
                newName: "Agendamentos");

            migrationBuilder.RenameIndex(
                name: "IX_Agendamento_TipoExameId",
                table: "Agendamentos",
                newName: "IX_Agendamentos_TipoExameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Agendamentos",
                table: "Agendamentos",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_TipoExames_TipoExameId",
                table: "Agendamentos",
                column: "TipoExameId",
                principalTable: "TipoExames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_TipoExames_TipoExameId",
                table: "Agendamentos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Agendamentos",
                table: "Agendamentos");

            migrationBuilder.RenameTable(
                name: "Agendamentos",
                newName: "Agendamento");

            migrationBuilder.RenameIndex(
                name: "IX_Agendamentos_TipoExameId",
                table: "Agendamento",
                newName: "IX_Agendamento_TipoExameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Agendamento",
                table: "Agendamento",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamento_TipoExames_TipoExameId",
                table: "Agendamento",
                column: "TipoExameId",
                principalTable: "TipoExames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
