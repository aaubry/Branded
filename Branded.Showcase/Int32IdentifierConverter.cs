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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Branded.SourceGenerator.UnitTests;

public class Int32IdentifierConverter<TBranded, TFactory> : JsonConverter<TBranded>
    where TFactory : IBrandedValueConverter<TBranded, int>, new()
{
    private readonly TFactory factory = new();

    public override TBranded? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetInt32();
        return factory.Wrap(value);
    }

    public override void Write(Utf8JsonWriter writer, TBranded value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(factory.Unwrap(value));
    }
}
