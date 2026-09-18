using System;
using System.ComponentModel;
using System.Reflection;

namespace Persistence.Common
{
    /// <summary>
    /// Lee el <see cref="DescriptionAttribute"/> de los enums del dominio, que es
    /// donde viven los nombres presentables de instituciones y áreas funcionales.
    /// </summary>
    internal static class Descripciones
    {
        public static string De<TEnum>(TEnum valor) where TEnum : struct, Enum
        {
            var nombre = valor.ToString();
            if (nombre is null)
            {
                return string.Empty;
            }

            var campo = typeof(TEnum).GetField(nombre, BindingFlags.Public | BindingFlags.Static);
            var atributo = campo?.GetCustomAttribute<DescriptionAttribute>();

            return atributo?.Description ?? nombre;
        }
    }
}
