namespace Application.Common
{
    /// <summary>
    /// Parámetros de paginación comunes a los listados. Normaliza los valores
    /// recibidos para que una petición maliciosa no pueda pedir la tabla completa.
    /// </summary>
    public class ConsultaPaginada
    {
        public const int TamanoPaginaMaximo = 100;

        private int _pagina = 1;
        private int _tamanoPagina = 20;

        public int Pagina
        {
            get => _pagina;
            set => _pagina = value < 1 ? 1 : value;
        }

        public int TamanoPagina
        {
            get => _tamanoPagina;
            set => _tamanoPagina = value switch
            {
                < 1 => 1,
                > TamanoPaginaMaximo => TamanoPaginaMaximo,
                _ => value
            };
        }

        public int Offset => (Pagina - 1) * TamanoPagina;
    }
}
