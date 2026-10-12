using System.Collections.Generic;
using Domain.Enums;

namespace Persistence.Seeding
{
    /// <summary>
    /// Estado en que el sembrador deja la cuenta en cada arranque, para que
    /// cada escenario del login se pueda repetir sin limpiar la base a mano.
    /// </summary>
    public enum EstadoCuentaDePrueba
    {
        /// <summary>Sin intentos fallidos ni bloqueo.</summary>
        Activa,

        /// <summary>Un intento fallido antes del tope: el siguiente error la bloquea.</summary>
        CasiBloqueada,

        /// <summary>Bloqueada hasta una fecha lejana: el login responde 423.</summary>
        Bloqueada,

        /// <summary>Con borrado lógico: el login responde 401 como si no existiera.</summary>
        Eliminada
    }

    public record PerfilDePrueba(
        string Correo,
        string Nombres,
        string Apellidos,
        TipoUsuario TipoUsuario,
        string? Cedula,
        string? Telefono,
        bool CorreoConfirmado,
        EstadoCuentaDePrueba Estado);

    /// <summary>
    /// Usuarios para probar el login en desarrollo. Todos usan el dominio
    /// reservado <c>.test</c> (RFC 2606), que nunca recibe correo real.
    /// </summary>
    public static class UsuariosDePrueba
    {
        /// <summary>
        /// Contraseña común a todas las cuentas. Cumple la política de Identity:
        /// 10+ caracteres, mayúscula, minúscula, dígito y símbolo.
        /// </summary>
        public const string Contrasena = "Prueba.Mide2026";

        public static readonly IReadOnlyList<PerfilDePrueba> Todos =
        [
            new("militar.activo@mide.test", "Juan Carlos", "Pérez Almonte",
                TipoUsuario.MILITAR_ACTIVO, "001-0000001-1", "809-555-0101",
                CorreoConfirmado: true, EstadoCuentaDePrueba.Activa),

            new("militar.retirado@mide.test", "Ramón Antonio", "Guzmán Reyes",
                TipoUsuario.MILITAR_RETIRADO, "001-0000002-2", "809-555-0102",
                CorreoConfirmado: true, EstadoCuentaDePrueba.Activa),

            // Sin cédula ni teléfono: comprueba que la app tolera los campos nulos.
            new("familiar@mide.test", "María Isabel", "Santos de Pérez",
                TipoUsuario.FAMILIAR, null, null,
                CorreoConfirmado: true, EstadoCuentaDePrueba.Activa),

            // El login no exige correo confirmado; la app recibe correoConfirmado = false.
            new("ciudadano@mide.test", "Ana Lucía", "Martínez Rosario",
                TipoUsuario.CIUDADANO, "001-0000004-4", "829-555-0104",
                CorreoConfirmado: false, EstadoCuentaDePrueba.Activa),

            new("casi.bloqueado@mide.test", "Pedro José", "Núñez Castillo",
                TipoUsuario.MILITAR_ACTIVO, "001-0000005-5", "809-555-0105",
                CorreoConfirmado: true, EstadoCuentaDePrueba.CasiBloqueada),

            new("bloqueado@mide.test", "Luis Miguel", "Féliz Batista",
                TipoUsuario.CIUDADANO, "001-0000006-6", "849-555-0106",
                CorreoConfirmado: true, EstadoCuentaDePrueba.Bloqueada),

            new("eliminado@mide.test", "Carmen Rosa", "Vásquez Mejía",
                TipoUsuario.FAMILIAR, "001-0000007-7", null,
                CorreoConfirmado: true, EstadoCuentaDePrueba.Eliminada)
        ];
    }
}
