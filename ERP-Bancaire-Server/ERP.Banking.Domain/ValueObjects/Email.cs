using System.Text.RegularExpressions;
using ERP.Banking.Domain.Exceptions;

namespace ERP.Banking.Domain.ValueObjects;

/// <summary>
/// Strongly-typed value object representing a validated email address.
/// </summary>
public sealed class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    /// <summary>
    /// Validates and creates an Email value object.
    /// Throws <see cref="DomainException"/> if the format is invalid.
    /// </summary>
    public static Email From(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalized = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
            throw new DomainException($"'{email}' is not a valid email address.");

        return new Email(normalized);
    }

    public bool Equals(Email? other) =>
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) =>
        obj is Email other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}