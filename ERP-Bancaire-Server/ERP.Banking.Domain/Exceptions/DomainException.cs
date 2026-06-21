namespace ERP.Banking.Domain.Exceptions;

/// <summary>
/// Base class for all domain-level exceptions.
/// Throw this (or a derived type) when a business rule is violated.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}