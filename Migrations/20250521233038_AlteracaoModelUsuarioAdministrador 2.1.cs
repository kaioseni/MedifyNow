using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedifyNow.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoModelUsuarioAdministrador21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailTemporario",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailTemporario",
                table: "Usuarios");
        }
    }
}
