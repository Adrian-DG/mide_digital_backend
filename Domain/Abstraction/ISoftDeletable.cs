namespace Domain.Abstraction
{
    /// <summary>
    /// Entidad con borrado lógico. Se declara aparte de
    /// <see cref="BaseEntityMetadata"/> porque el usuario hereda su identificador
    /// de Identity y no puede heredar la clase base, pero sí debe respetar el
    /// mismo filtro de borrado.
    /// </summary>
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
    }
}
