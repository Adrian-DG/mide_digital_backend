using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Domain.Common
{
    /// <summary>
    /// Construye los fragmentos alfanuméricos usados en los códigos visibles
    /// de servicios y solicitudes (sin acentos, sin espacios, en mayúsculas).
    /// </summary>
    public static class Secuencias
    {
        public const int LongitudFragmento = 8;

        public static string Fragmento(string? texto, int longitud = LongitudFragmento)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return string.Empty;
            }

            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var limpio = new StringBuilder(normalizado.Length);

            foreach (var caracter in normalizado)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(caracter))
                {
                    limpio.Append(char.ToUpperInvariant(caracter));
                }
            }

            var resultado = limpio.ToString();
            return resultado.Length <= longitud ? resultado : resultado[..longitud];
        }
    }
}
