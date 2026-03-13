using System;

namespace Branded;

/// <summary>
/// Configures how a string-branded type is mapped to the database.
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class DbStringType : global::System.Attribute
{
    public DbStringType(bool isAnsi)
    {
        IsAnsi = isAnsi;
    }

    public DbStringType(int length, bool isFixedLength = false, bool isAnsi = false) : this(isAnsi)
    {
        Length = length;
        IsFixedLength = isFixedLength;
    }

    public int? Length { get; }
    public bool IsFixedLength { get; }
    public bool IsAnsi { get; }
}
