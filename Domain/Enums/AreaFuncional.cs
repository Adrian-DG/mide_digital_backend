using System.ComponentModel;

namespace Domain.Enums
{
    public enum AreaFuncional
    {
        [Description("Salud")]
        SALUD = 1,
        [Description("Educación y Formación")]
        EDUCACION = 2,
        [Description("Seguridad")]
        SEGURIDAD = 3,
        [Description("Bienestar y Pensiones")]
        BIENESTAR = 4,
        [Description("Innovación")]
        INNOVACION = 5,
        [Description("Medio Ambiente")]
        MEDIO_AMBIENTE = 6
    }
}
