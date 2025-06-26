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

using System;

namespace Branded;

/// <summary>
/// Associates a custom attribute to each branded type.
/// </summary>
/// <param name="attributeType">The type of the custom attribute. It must inherit from <see cref="System.Attribute"/>.</param>
/// <param name="arguments">
/// A list of constructor arguments to pass to the attribute.
/// If you need to reference the current type, use <code>typeof(BrandedTypePlaceholder)</code>.
/// If you need to reference the current type's inner type, use <code>typeof(BrandedInnerTypePlaceholder)</code>.
/// </param>
/// <example>
/// [assembly:SourceGeneratorCustomAttribute(
///     typeof(Newtonsoft.Json.JsonConverter),
///     typeof(MyNamespace.MyJsonConverter&lt;BrandedTypePlaceholder&gt;)
/// )]
/// </example>
partial class SourceGeneratorCustomAttributeAttribute(Type attributeType)
{
    public Type AttributeType { get; } = attributeType;

    /// <summary>
    /// The list of constructor arguments that that should be passed to the custom attribute.
    /// </summary>
    public object[]? ConstructorArguments { get; set; }

    /// <summary>
    /// The list of named arguments (properties) that should be passed to the custom attribute.
    /// Each argument should consist of a string with the name of the property, followed by the value of that property.
    /// If you need to reference the current type, use typeof(BrandedTypePlaceholder).
    /// If you need to reference the current type's inner type, use <code>typeof(BrandedInnerTypePlaceholder)</code>.
    /// </summary>
    public object[]? NamedArguments { get; set; }

    /// <summary>
    /// When specified, restricts the application of the attribute to branded types whose inner type is present in this list.
    /// </summary>
    public Type[]? OnlyForInnerTypes { get; set; }

    /// <summary>
    /// When specified, restricts the application of the attribute to branded types whose inner type is NOT present in this list.
    /// </summary>
    public Type[]? ExceptForInnerTypes { get; set; }
}

/// <summary>
/// A type that represents the currently branded type.
/// </summary>
partial class BrandedTypePlaceholder { }

/// <summary>
/// A type that represents the currently branded type's inner type.
/// </summary>
partial class BrandedInnerTypePlaceholder { }

/// <summary>
/// A type that represents the currently branded type's <see cref="IBrandedValueConverter">.
/// </summary>
partial class BrandedConverterTypePlaceholder { }