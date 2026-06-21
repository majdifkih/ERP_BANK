namespace ERP.Banking.Domain.Exceptions;

/// <summary>
/// Thrown when a requested entity does not exist in the data store.
/// </summary>
public sealed class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.") { }
}