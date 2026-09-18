using System.ComponentModel;

namespace Domain.Enums
{
    public enum Institucion
    {
        [Description("Ministerio de Defensa")]
        MIDE = 1,
        [Description("Ejército de República Dominicana")]
        ERD = 2,
        [Description("Armada de República Dominicana")] 
        ARD = 3,
        [Description("Fuerza Aérea de República Dominicana")]
        FARD = 4

    }
}