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

using Branded.Integrations.Dapper;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Branded.Showcase.Dapper;

public record Widget(
    WidgetIdentifier Id,
    UserIdentifier Owner
);

public readonly partial record struct WidgetIdentifier(int Id);
public readonly partial record struct UserIdentifier(string Username);

public static class Program
{
    /*
    CREATE TABLE [dbo].[Widgets](
        [Id] [int] NOT NULL,
        [Owner] [nvarchar](50) NOT NULL
    )
    */

    public static void Main()
    {
        // Register the type handlers
        WidgetIdentifier.Dispatch(BrandedTypeRegistry.Instance);
        UserIdentifier.Dispatch(BrandedTypeRegistry.Instance);

        using var db = new SqlConnection("Data source=localhost; Integrated Security=SSPI; Initial Catalog=Sandbox; TrustServerCertificate=true");
        db.Execute(
            """
            DELETE FROM Widgets
            
            INSERT INTO Widgets(Id, Owner) VALUES (1, 'alex'),(2, 'alex'),(3, 'rain')
            """
        );

        var owner = new UserIdentifier("alex");

        var results = db.Query<Widget>(
            """
            SELECT Id, Owner
            FROM Widgets
            WHERE Owner = @owner
            """,
            new { owner }
        );

        foreach (var result in results)
        {
            Console.WriteLine(result);
        }
    }
}
