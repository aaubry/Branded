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

namespace Branded.Showcase;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("Newtonsoft.Json");
        Console.WriteLine("===============");
        Branded.Showcase.Newtonsoft.Json.Program.Main();

        Console.WriteLine();
        Console.WriteLine("System.Text.Json");
        Console.WriteLine("================");
        Branded.Showcase.System.Text.Json.Program.Main();

        Console.WriteLine();
        Console.WriteLine("Dapper");
        Console.WriteLine("======");
        Branded.Showcase.Dapper.Program.Main();
    }
}
