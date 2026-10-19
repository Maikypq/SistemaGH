using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGH.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuitarUsuarioSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "Correo", "NombreCompleto", "PasswordHash", "PerfilId" },
                values: new object[] { 1, true, "admin@sistemagh.com", "Administrador del Sistema", "AQAAAAIAAYagAAAAEP2W9r5J7v8xZ3qL1mN6bV4cX5zQ8wE2rT1yU3iO4pA7sD6fG9hJ0kL2zX1cV3bN5m==", 1 });
        }
    }
}
