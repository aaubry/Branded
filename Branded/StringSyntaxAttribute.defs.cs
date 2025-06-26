// Branded - Branded types for C#
// Copyright (C) 2025 Antoine Aubry

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Fake version of the StringSyntaxAttribute, which was introduced in .NET 7</summary>
internal sealed class StringSyntaxAttribute : Attribute
{
    /// <summary>The syntax identifier for strings containing regular expressions.</summary>
    public const string Regex = nameof(Regex);

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSyntaxAttribute"/> class.
    /// </summary>
    public StringSyntaxAttribute(string syntax)
    {
    }
}
