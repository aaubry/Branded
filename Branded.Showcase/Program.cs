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

using Branded.Showcase;
using Branded.SourceGenerator.UnitTests;
using System.Text.Json;
using System.Text.Json.Serialization;

[assembly: Branded.SourceGeneratorConventions(BrandedTypeNamePattern = @"Identifier$")]
[assembly: Branded.SourceGeneratorCustomAttribute(
    typeof(JsonConverter),
    ConstructorArguments = [
        new[]
        {
            typeof(Int32IdentifierConverter<,>),
            typeof(Branded.BrandedTypePlaceholder),
            typeof(Branded.BrandedConverterTypePlaceholder)
        },
    ],
    OnlyForInnerTypes = [
        typeof(int)
    ]
)]
[assembly: Branded.SourceGeneratorCustomAttribute(
    typeof(JsonConverter),
    ConstructorArguments = [
        new[]
        {
            typeof(StringIdentifierConverter<,>),
            typeof(Branded.BrandedTypePlaceholder),
            typeof(Branded.BrandedConverterTypePlaceholder)
        }
    ],
    OnlyForInnerTypes = [
        typeof(string)
    ]
)]
[assembly: Branded.SourceGeneratorCustomAttribute(
    typeof(JsonNumberHandlingAttribute),
    ConstructorArguments = [
        JsonNumberHandling.Strict
    ]
)]

var json = JsonSerializer.Serialize(new Widget(new(42), new("alex")));

Console.WriteLine($"json: {json}");

var parsed = JsonSerializer.Deserialize<Widget>(json);
Console.WriteLine($"parsed: {parsed}");
