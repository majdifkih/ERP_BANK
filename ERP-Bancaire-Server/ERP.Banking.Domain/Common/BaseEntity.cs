namespace ERP.Banking.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Provides a strongly-typed identity and common audit fields.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; protected set; }

    protected void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}