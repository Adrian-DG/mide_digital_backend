namespace Persistence.Common
{
    internal static class Sql
    {
        /// <summary>
        /// Neutraliza los comodines de LIKE para que un término con %, _ o [
        /// no devuelva el conjunto completo.
        /// </summary>
        public static string EscaparLike(string termino)
            => termino.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
    }
}
