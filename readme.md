# Branded types for C#

## Overview

**Branded.SourceGenerator** is a C# source generator that simplifies the creation and use of branded types.
Branded types offer a robust way to eliminate a class of bugs by wrapping primitive types, like `int` or `string`,
with more specific types. This enhances type safety, making it difficult to mix identifiers unintentionally and
allowing the compiler to catch errors that would otherwise go unnoticed.

### What are Branded Types?

Branded types are custom types that encapsulate a primitive type. By branding identifiers, you can enforce stronger type checks in your application. For instance:

```csharp
public readonly partial record struct UserIdentifier(int Id);
public readonly partial record struct GroupIdentifier(int Id);
```

Using branded types ensures that you can't accidentally pass a `UserIdentifier` to a function expecting a `GroupIdentifier`, and vice versa.

### Features of the Source Generator

When you declare a `readonly partial record struct`, the source generator automatically enhances the type with the following features:

- **`ToString()` Method**: Provides a string representation of the underlying value.
- **Implicit Conversion to Underlying Type**: Supports seamless conversion from the branded type to its underlying type (e.g., from `UserIdentifier` to `int`).
- **Interface Implementations**: Implements interfaces like `IComparable`, `IComparable<T>`, and `IFormattable` if the underlying type supports them.
- **Custom Attributes**: Automatically applies any configured custom attributes to allow extending the types with custom converters or other similar facilities.

### Getting Started

To use the source generator, add the **Branded.SourceGenerator** package to your project:

```shell
dotnet add package Branded.SourceGenerator
```

Optionally, you can add the **Branded** package for additional configuration options:

```shell
dotnet add package Branded
```

### Example Usage

#### Declare a Branded Type

Here's how you can declare and use a branded type with the source generator:

```csharp
public readonly partial record struct WidgetIdentifier(int Id);
```

This will generate additional functionality for WidgetIdentifier:
```csharp
partial record struct WidgetIdentifier
    : global::System.IComparable
    , global::System.IComparable<global::BrandedTest.WidgetIdentifier>
    , global::System.IFormattable
{
    public override string ToString() => Id.ToString();

    public static implicit operator int(global::BrandedTest.WidgetIdentifier value) => value.Id;

    int global::System.IComparable.CompareTo(object other) => CompareTo((global::BrandedTest.WidgetIdentifier)other);

    public int CompareTo(global::BrandedTest.WidgetIdentifier other) => Id.CompareTo(other.Id);

    public string ToString(string format, global::System.IFormatProvider formatProvider) => Id.ToString(format, formatProvider);
}
```

#### Use the Branded Type

```csharp
void ProcessWidget(WidgetIdentifier widgetId)
{
    Console.WriteLine($"Processing widget with ID: {widgetId}");
}

var widgetId = new WidgetIdentifier(42);
ProcessWidget(widgetId);

// Implicit conversion to int is allowed
int underlyingId = widgetId;
```

## Configuration

### Custom Conventions

Use `SourceGeneratorConventionsAttribute` to set naming patterns for your branded types. For instance, to ensure all branded types end with "Identifier":

```csharp
[assembly: Branded.SourceGeneratorConventions(BrandedTypeNamePattern = "Identifier$")]
```

The supported properties are:

| Property name          | Description                                                                                                                                                          |
|------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| BrandedTypeNamePattern | A regular expression that selects the type names that should be treated as branded types. Only type names matching that expression will be treated as branded types. |

### Custom Attributes

Use `SourceGeneratorCustomAttributeAttribute` to apply custom attributes to your branded types. This is useful for things like JSON serialization:

```csharp
[assembly: Branded.SourceGeneratorCustomAttribute(
    typeof(JsonConverterAttribute),
    typeof(IdentifierConverter<Branded.BrandedTypePlaceholder, Branded.BrandedInnerTypePlaceholder>)
)]
```

In the above example, we are using `Branded.BrandedTypePlaceholder` and `Branded.BrandedInnerTypePlaceholder` to refer to the current branded type and to its underlying type, respectively. If we had the following branded type:
```csharp
public readonly partial record struct WidgetIdentifier(int Id);
```
the following attribute would be applied to it:
```csharp
[JsonConverter(typeof(IdentifierConverter<WidgetIdentifier, int>))]
```

#### Specifying constructor arguments

The `ConstructorArguments` can be used to specify the values of the constructor arguments of the custom attribute.
For example,

```csharp
[assembly: Branded.SourceGeneratorCustomAttribute(
    typeof(Category),
    ConstructorArguments = [
        "Identifiers"
    ]
)]
```
would add the following attribute to each branded type:
```csharp
[Category("Identifiers")]
```

#### Specifying named arguments

The `NamedArguments` property can be used to specify named arguments (properties) of the custom attribute. The array should contain one pair of element for each property. The first element of the pair is the name of the property
and the second element is the value of the pair. For example,

```csharp
[assembly: Branded.SourceGeneratorCustomAttribute(
    typeof(JsonObject),
    NamedArguments = [
        // First property
        nameof(JsonObject.Title),
        "Identifier JSON Object",
        // Second property
        nameof(JsonObject.ItemTypeNameHandling),
        TypeNameHandling.All
    ]
)]
```
would add the following attribute to each branded type:
```csharp
[JsonObject(Title = "Identifier JSON Object", ItemTypeNameHandling = TypeNameHandling.All)]
```

#### Restricting the application of the custom attribute

In some cases it may be desirable to apply the custom attribute to specific kinds of branded types. For example, if we want to associate a JSON converter that only supports integers, we don't want to apply it to a branded type whose underlying type is string.

The `OnlyForInnerTypes` and `ExceptForInnerTypes` properties allow to specify the list of inner types to which the attribute applies or to which the attribute doesn't apply, respectively.

For example, to associate our integer identifier converter, we can use the following syntax:

```csharp
[assembly: Branded.SourceGeneratorCustomAttribute(
    typeof(JsonConverter),
    ConstructorArguments = [
        typeof(Int32IdentifierConverter)
    ],
    OnlyForInnerTypes = [
        typeof(int)
    ]
)]
```
It will add the following attribute to each branded type whose underlying type is `int`:
```csharp
[JsonConverter(typeof(Int32IdentifierConverter))]
```

#### Referring to the current type

Sometimes, you want to refer to some aspect of the branded type being generated. 
The library defines the following special type placeholders
that can be used to refer to the current branded type elements:

| Type name                       | Usage                                                                         |
|---------------------------------|-------------------------------------------------------------------------------|
| BrandedTypePlaceholder          | Replaced by the current branded type.                                         |
| BrandedInnerTypePlaceholder     | Replaced by the current branded type's inner type.                            |
| BrandedConverterTypePlaceholder | Replaced by the current branded type's IBrandedValueConverter implementation. |

For example, you may want to associate a JSON converter to each branded type. The converter may need to know the
type it is associated with. Your converter may look like this:

```csharp
public class IdentifierConverter<TBranded> : JsonConverter<TBranded>
{
    public override TBranded? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Omitted
    }

    public override void Write(Utf8JsonWriter writer, TBranded value, JsonSerializerOptions options)
    {
        // Omitted
    }
}
```

To associate that converter to every branded type, you can use `BrandedTypePlaceholder`:
```csharp
[assembly: Branded.SourceGeneratorCustomAttribute(
    typeof(JsonConverter),
    ConstructorArguments = [
        typeof(IdentifierConverter<Branded.BrandedTypePlaceholder>)
    ]
)]
```

If you have the following branded types,
```csharp
public readonly partial record struct WidgetIdentifier(int Id);
public readonly partial record struct UserIdentifier(string Username);
```
they will have the following attributes, respectively:
```csharp
[JsonConverter(typeof(IdentifierConverter<WidgetIdentifier>))]
[JsonConverter(typeof(IdentifierConverter<UserIdentifier>))]
```

You may also want to pass the inner type of the each branded type so you don't need to use reflection to discover it. For that you can use `BrandedInnerTypePlaceholder`:
```csharp
[assembly: Branded.SourceGeneratorCustomAttribute(
    typeof(JsonConverter),
    ConstructorArguments = [
        typeof(IdentifierConverter<Branded.BrandedTypePlaceholder, Branded.BrandedInnerTypePlaceholder>)
    ]
)]
```
The branded types from the previous example will have the following attributes, respectively:
```csharp
[JsonConverter(typeof(IdentifierConverter<WidgetIdentifier, int>))]
[JsonConverter(typeof(IdentifierConverter<UserIdentifier, string>))]
```

#### Statically typed conversion

Each branded type has an inner class named `Converter` that implements the `IBrandedValueConverter<,>` interface.
It has methods to convert between a branded type and its underlying value.
This can be used to implement a custom converter without having to resort to reflection.

Here is an example implementation of a JSON converter:
```csharp
// Only works for int but could easly be extended / replicated for other types
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
```

We can associate that converter to each branded type:
```csharp
[assembly: Branded.SourceGeneratorCustomAttribute(
    typeof(JsonConverter),
    ConstructorArguments = [
        typeof(Int32IdentifierConverter<Branded.BrandedTypePlaceholder, Branded.BrandedConverterTypePlaceholder>)
    ]
)]
```

Actually the above code will result in a compilation error because `Int32IdentifierConverter` requires that its second type argument implements `IBrandedValueConverter` and `BrandedConverterTypePlaceholder` doesn't. To overcome this problem, instead of passing a constructed generic type, we can pass an array of types, where the first element is an open generic type, and the remaining elements are its generic arguments. The source generator recognizes this pattern and uses it to construct the generic type:
```csharp
[assembly: Branded.SourceGeneratorCustomAttribute(
    typeof(JsonConverter),
    ConstructorArguments = [
        new[]
        {
            typeof(Int32IdentifierConverter<,>),
            typeof(Branded.BrandedTypePlaceholder),
            typeof(Branded.BrandedConverterTypePlaceholder)
        }
    ]
)]
```
The WidgetIdentifier from the previous example will have the following attribute:
```csharp
[JsonConverter(typeof(Int32IdentifierConverter<WidgetIdentifier, WidgetIdentifier.Converter>))]
```

## Errors and information messages

This section describes the error and information codes that are produced by the source generator.

### BRND000

**Severity**: Info  

Notifies that the branded type has been correctly generated. This informational message confirms that the source generator has successfully processed and augmented the branded type as expected.

### BRND001

**Severity**: Error  

A branded type has been identified but is missing the `partial` modifier. To allow the source generator to augment the type with additional functionality, ensure that the type is declared as `partial`.

### BRND002

**Severity**: Error  

A branded type must have a constructor taking a single parameter. This ensures that branded types encapsulate a single underlying value, such as an `int` or `string`.

### BRND003

**Severity**: Error  

A configuration parameter is invalid. This error indicates that a parameter used in configuring the source generator does not meet the required criteria or expected format.

### BRND004

**Severity**: Warning  

A configuration parameter is not supported. While the parameter has been recognized, it is not supported in the current version of the source generator, and its effects will be ignored. This most likely indicates a mismatch between the `Branded.SourceGenerator` and `Branded` packages. Make sure to use the same version of both packages.

## License

This project is licensed under the MIT License. See the [license.md](license.md) file for more details.
