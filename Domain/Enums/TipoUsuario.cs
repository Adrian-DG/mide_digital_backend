using System.ComponentModel;

namespace Domain.Enums
{
    public enum TipoUsuario
    {
        [Description("Miembro activo de las Fuerzas Armadas")]
        MILITAR_ACTIVO = 1,
        [Description("Miembro retirado de las Fuerzas Armadas")]
        MILITAR_RETIRADO = 2,
        [Description("Familiar de miembro de las Fuerzas Armadas")]
        FAMILIAR = 3,
        [Description("Ciudadano")]
        CIUDADANO = 4
    }
}
