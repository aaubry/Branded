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

namespace Branded.Showcase.System.Text.Json;

public record Widget(
    WidgetIdentifier Id,
    UserIdentifier Owner
);

public readonly partial record struct WidgetIdentifier(int Id);
public readonly partial record struct UserIdentifier(string Username);

// Not a branded type according to the defined SourceGeneratorConventions
public readonly partial record struct WidgetCode(string Code);

public static class Program
{
    public static void Main()
    {

        var json = JsonSerializer.Serialize(new Widget(new(42), new("alex")));
        Console.WriteLine($"json: {json}");

        var parsed = JsonSerializer.Deserialize<Widget>(json);
        Console.WriteLine($"parsed: {parsed}");
    }
}
