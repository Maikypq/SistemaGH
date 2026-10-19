using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaGH.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDataSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Perfiles",
                columns: new[] { "Id", "Activo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Perfil con acceso total al sistema", "Administrador" },
                    { 2, true, "Perfil operativo estándar", "Operador" }
                });

            migrationBuilder.InsertData(
                table: "Permisos",
                columns: new[] { "Id", "Accion", "CodigoSistema", "Modulo" },
                values: new object[,]
                {
                    { 1, "Ver Listado", "Usuarios.Ver", "Usuarios" },
                    { 2, "Crear Usuario", "Usuarios.Crear", "Usuarios" },
                    { 3, "Editar Usuario", "Usuarios.Editar", "Usuarios" },
                    { 4, "Eliminar Usuario", "Usuarios.Eliminar", "Usuarios" },
                    { 5, "Ver Perfiles", "Perfiles.Ver", "Perfiles" },
                    { 6, "Gestionar Permisos", "Perfiles.GestionarPermisos", "Perfiles" },
                    { 7, "Ver Empleados", "Empleados.Ver", "Empleados" },
                    { 8, "Crear Empleado", "Empleados.Crear", "Empleados" }
                });

            migrationBuilder.InsertData(
                table: "PerfilPermisos",
                columns: new[] { "PerfilId", "PermisoId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 1, 4 },
                    { 1, 5 },
                    { 1, 6 },
                    { 1, 7 },
                    { 1, 8 }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "Correo", "NombreCompleto", "PasswordHash", "PerfilId" },
                values: new object[] { 1, true, "admin@sistemagh.com", "Administrador del Sistema", "AQAAAAIAAYagAAAAEP2W9r5J7v8xZ3qL1mN6bV4cX5zQ8wE2rT1yU3iO4pA7sD6fG9hJ0kL2zX1cV3bN5m==", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PerfilPermisos",
                keyColumns: new[] { "PerfilId", "PermisoId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "PerfilPermisos",
                keyColumns: new[] { "PerfilId", "PermisoId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "PerfilPermisos",
                keyColumns: new[] { "PerfilId", "PermisoId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                table: "PerfilPermisos",
                keyColumns: new[] { "PerfilId", "PermisoId" },
                keyValues: new object[] { 1, 4 });

            migrationBuilder.DeleteData(
                table: "PerfilPermisos",
                keyColumns: new[] { "PerfilId", "PermisoId" },
                keyValues: new object[] { 1, 5 });

            migrationBuilder.DeleteData(
                table: "PerfilPermisos",
                keyColumns: new[] { "PerfilId", "PermisoId" },
                keyValues: new object[] { 1, 6 });

            migrationBuilder.DeleteData(
                table: "PerfilPermisos",
                keyColumns: new[] { "PerfilId", "PermisoId" },
                keyValues: new object[] { 1, 7 });

            migrationBuilder.DeleteData(
                table: "PerfilPermisos",
                keyColumns: new[] { "PerfilId", "PermisoId" },
                keyValues: new object[] { 1, 8 });

            migrationBuilder.DeleteData(
                table: "Perfiles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Perfiles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
