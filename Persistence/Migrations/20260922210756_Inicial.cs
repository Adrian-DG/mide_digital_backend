using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "retroalimentacion");

            migrationBuilder.EnsureSchema(
                name: "catalogo");

            migrationBuilder.EnsureSchema(
                name: "tramites");

            migrationBuilder.EnsureSchema(
                name: "contenido");

            migrationBuilder.EnsureSchema(
                name: "identidad");

            migrationBuilder.CreateTable(
                name: "CategoriasServicio",
                schema: "catalogo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasServicio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dependencias",
                schema: "catalogo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Siglas = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Institucion = table.Column<int>(type: "int", nullable: false),
                    AreaFuncional = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dependencias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Noticias",
                schema: "contenido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Resumen = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Fuente = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EsDestacada = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Noticias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                schema: "identidad",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoUsuario = table.Column<int>(type: "int", nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                schema: "catalogo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EstaDisponible = table.Column<bool>(type: "bit", nullable: false),
                    CategoriaServicioId = table.Column<int>(type: "int", nullable: false),
                    DependenciaId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servicios_CategoriasServicio_CategoriaServicioId",
                        column: x => x.CategoriaServicioId,
                        principalSchema: "catalogo",
                        principalTable: "CategoriasServicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Servicios_Dependencias_DependenciaId",
                        column: x => x.DependenciaId,
                        principalSchema: "catalogo",
                        principalTable: "Dependencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosClaims",
                schema: "identidad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosClaims_Usuarios_UserId",
                        column: x => x.UserId,
                        principalSchema: "identidad",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosLogins",
                schema: "identidad",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UsuariosLogins_Usuarios_UserId",
                        column: x => x.UserId,
                        principalSchema: "identidad",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosTokens",
                schema: "identidad",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UsuariosTokens_Usuarios_UserId",
                        column: x => x.UserId,
                        principalSchema: "identidad",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalificacionesServicio",
                schema: "retroalimentacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estrellas = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaCalificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionesServicio", x => x.Id);
                    table.CheckConstraint("CK_CalificacionesServicio_Estrellas", "[Estrellas] BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_CalificacionesServicio_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalSchema: "catalogo",
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesServicio_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "identidad",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Requisitos",
                schema: "catalogo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EsObligatorio = table.Column<bool>(type: "bit", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requisitos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requisitos_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalSchema: "catalogo",
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes",
                schema: "tramites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaProcesamiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalSchema: "catalogo",
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "identidad",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosSolicitud",
                schema: "tramites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    RequisitoId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosSolicitud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosSolicitud_Requisitos_RequisitoId",
                        column: x => x.RequisitoId,
                        principalSchema: "catalogo",
                        principalTable: "Requisitos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosSolicitud_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalSchema: "tramites",
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesServicio_Servicio_Usuario",
                schema: "retroalimentacion",
                table: "CalificacionesServicio",
                columns: new[] { "ServicioId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesServicio_UsuarioId",
                schema: "retroalimentacion",
                table: "CalificacionesServicio",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UX_CategoriasServicio_Nombre",
                schema: "catalogo",
                table: "CategoriasServicio",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dependencias_AreaFuncional",
                schema: "catalogo",
                table: "Dependencias",
                column: "AreaFuncional");

            migrationBuilder.CreateIndex(
                name: "UX_Dependencias_Siglas",
                schema: "catalogo",
                table: "Dependencias",
                column: "Siglas",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosSolicitud_RequisitoId",
                schema: "tramites",
                table: "DocumentosSolicitud",
                column: "RequisitoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosSolicitud_Solicitud_Requisito",
                schema: "tramites",
                table: "DocumentosSolicitud",
                columns: new[] { "SolicitudId", "RequisitoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Noticias_EsDestacada",
                schema: "contenido",
                table: "Noticias",
                column: "EsDestacada");

            migrationBuilder.CreateIndex(
                name: "IX_Noticias_FechaPublicacion",
                schema: "contenido",
                table: "Noticias",
                column: "FechaPublicacion");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitos_Servicio_Obligatorio",
                schema: "catalogo",
                table: "Requisitos",
                columns: new[] { "ServicioId", "EsObligatorio" });

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_Categoria_Disponible",
                schema: "catalogo",
                table: "Servicios",
                columns: new[] { "CategoriaServicioId", "EstaDisponible" });

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_DependenciaId",
                schema: "catalogo",
                table: "Servicios",
                column: "DependenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_Nombre",
                schema: "catalogo",
                table: "Servicios",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_CreatedAt",
                schema: "tramites",
                table: "Solicitudes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_Servicio_Estado",
                schema: "tramites",
                table: "Solicitudes",
                columns: new[] { "ServicioId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_Usuario_Estado",
                schema: "tramites",
                table: "Solicitudes",
                columns: new[] { "UsuarioId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "identidad",
                table: "Usuarios",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Cedula",
                schema: "identidad",
                table: "Usuarios",
                column: "Cedula");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TipoUsuario",
                schema: "identidad",
                table: "Usuarios",
                column: "TipoUsuario");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "identidad",
                table: "Usuarios",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosClaims_UserId",
                schema: "identidad",
                table: "UsuariosClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosLogins_UserId",
                schema: "identidad",
                table: "UsuariosLogins",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalificacionesServicio",
                schema: "retroalimentacion");

            migrationBuilder.DropTable(
                name: "DocumentosSolicitud",
                schema: "tramites");

            migrationBuilder.DropTable(
                name: "Noticias",
                schema: "contenido");

            migrationBuilder.DropTable(
                name: "UsuariosClaims",
                schema: "identidad");

            migrationBuilder.DropTable(
                name: "UsuariosLogins",
                schema: "identidad");

            migrationBuilder.DropTable(
                name: "UsuariosTokens",
                schema: "identidad");

            migrationBuilder.DropTable(
                name: "Requisitos",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "Solicitudes",
                schema: "tramites");

            migrationBuilder.DropTable(
                name: "Servicios",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "Usuarios",
                schema: "identidad");

            migrationBuilder.DropTable(
                name: "CategoriasServicio",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "Dependencias",
                schema: "catalogo");
        }
    }
}
