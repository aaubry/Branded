# Library integrations for Branded.SourceGenerator

## Overview

**Branded.Integrations** is a C# source package that complements the [Branded.SourceGenerator](https://www.nuget.org/packages/Branded.SourceGenerator) package by adding converters for a few common libraries. The goal is to make using branded types as seamless as possible. For example, when deserializing a JSON document, the properties that are identifiers can be of a branded type instead of being `int` or `string`.

This package is distributed as a source-only package, which means that the source code of the integrations is directly added to your project. The advantage of doing so is that the source files also apply the assembly attributes needed to add the integration, making this library plug-and-play.

## List of integrated libraries

The following is a list of all the library integrations that are included in this package. The package uses custom msbuild conditions to ensure that only the integrations for the libraries that are in use by the project are included.

### System.Text.Json

The integration consists of a set of `JsonConverter` for branded types with the following underlying types: `int`, `uint`, `long`, `ulong` and `string`.

#### Example

```csharp
using System.Text.Json;

namespace Branded.Showcase;

public record Widget(
    WidgetIdentifier Id,
    UserIdentifier Owner
);

public readonly partial record struct WidgetIdentifier(int Id);
public readonly partial record struct UserIdentifier(string Username);

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
```

#### Output

```
json: {"Id":42,"Owner":"alex"}
parsed: Widget { Id = 42, Owner = alex }
```

## Newtonsoft.Json

The integration consists of a `JsonConverter` for branded types of any underlying type.

#### Example

```csharp
using Newtonsoft.Json;

namespace Branded.Showcase;

public record Widget(
    WidgetIdentifier Id,
    UserIdentifier Owner
);

public readonly partial record struct WidgetIdentifier(int Id);
public readonly partial record struct UserIdentifier(string Username);

public static class Program
{
    public static void Main()
    {

        var json = JsonConvert.SerializeObject(new Widget(new(42), new("alex")));
        Console.WriteLine($"json: {json}");

        var parsed = JsonConvert.DeserializeObject<Widget>(json);
        Console.WriteLine($"parsed: {parsed}");
    }
}
```

#### Output

```
json: {"Id":42,"Owner":"alex"}
parsed: Widget { Id = 42, Owner = alex }
```

## Dapper

The integration consists of an helper class, `BrandedTypeRegistry`, that registers type handlers for branded types. The registration must be performed manually by calling the `Dispatch` method on each branded type, passing `BrandedTypeRegistry.Instance`. Usually this is performed on the static constructor of the class that uses the branded types.

#### Example

```csharp
using Branded.Integrations.Dapper;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Branded.Showcase;

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
```

#### Output

```
Widget { Id = 1, Owner = alex }
Widget { Id = 2, Owner = alex }
```
