using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedifyNow.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoModelUsuarioAdministrador2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailTemporario",
                table: "Administrador",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailTemporario",
                table: "Administrador");
        }
    }
}
