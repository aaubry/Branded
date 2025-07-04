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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Text.RegularExpressions;

namespace Branded.SourceGenerator
{
    [Generator]
    public class BrandedTypeSourceGenerator : IIncrementalGenerator
    {
        private static readonly string sourceGeneratorConventionsAttributeTypeName = typeof(SourceGeneratorConventionsAttribute).FullName;
        private static readonly string sourceGeneratorCustomAttributeAttributeTypeName = typeof(SourceGeneratorCustomAttributeAttribute).FullName;

        private sealed class SourceGeneratorConfiguration
        {
            public Regex? BrandedTypeNamePattern { get; set; }
        }

        private sealed record CustomAttributeSettings(
            string Template,
            SmallList<string>? OnlyForInnerTypes,
            SmallList<string>? ExceptForInnerTypes
        );

        private static readonly SourceGeneratorConfiguration DefaultConfiguration = new();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var brandedAssemblyReferenced = context.CompilationProvider
                .Select((c, _) => c.ReferencedAssemblyNames.Any(a => a.Name == "Branded"));

            var configuration = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    sourceGeneratorConventionsAttributeTypeName,
                    Always,
                    ParseConventions
                );

            ReportDiagnostics(context, configuration);

            var customAttributes = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    sourceGeneratorCustomAttributeAttributeTypeName,
                    Always,
                    ParseCustomAttributes
                );

            ReportDiagnostics(context, customAttributes
                .Where(c => c is not null)
                .SelectMany((c, _) => c!)
            );

            var currentCustomAttributes = customAttributes
                .Where(a => a is not null)
                .SelectMany((a, _) => a!)
                .Select((a, _) => a.Value!)
                .Where(a => a is not null)
                .Collect();

            var identifierTypes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    IsBrandedTypeCandidate,
                    ParseBrandedTypeCandidate
                )
                .Where(s => s.Value is not null)
                .Combine(configuration.Collect())
                .Where(sc => sc.Right.Length == 0 || sc.Right[0].Value is not null) // Only when the configuration is valid or omitted
                .Select((sc, _) => new Result<(BrandedTypeCandidate brandedType, SourceGeneratorConfiguration configuration)>(
                    (sc.Left.Value!, sc.Right.FirstOrDefault()?.Value ?? DefaultConfiguration),
                    sc.Left.Diagnostics
                ))
                .Where(sc => sc.Value.configuration.BrandedTypeNamePattern is null || sc.Value.configuration.BrandedTypeNamePattern.IsMatch(sc.Value.brandedType.FullName));

            ReportDiagnostics(context, identifierTypes);

            context.RegisterSourceOutput(
                identifierTypes
                    .Where(sc => sc.Diagnostics is null)
                    .Combine(currentCustomAttributes)
                    .Combine(brandedAssemblyReferenced),
                (ctx, sc) => GenerateBrandedType(
                    ctx,
                    sc.Left.Left.Value.brandedType,
                    sc.Left.Left.Value.configuration,
                    sc.Left.Right,
                    sc.Right
                )
            );
        }

        private static void ReportDiagnostics<T>(
            IncrementalGeneratorInitializationContext context,
            IncrementalValuesProvider<Result<T>> results
        )
        {
            context.RegisterSourceOutput(
                results
                    .Where(c => c.Diagnostics is not null)
                    .SelectMany((c, _) => c.Diagnostics!),
                (ctx, d) => ctx.ReportDiagnostic(d)
            );
        }

        private static bool Always(SyntaxNode node, CancellationToken cancellationToken) => true;

        private static Result<SourceGeneratorConfiguration> ParseConventions(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            if (context.Attributes.Length != 1)
            {
                return Diagnostic.Create(
                    GeneratorDiagnosticDescriptors.InvalidConfiguration,
                    context.TargetNode.GetLocation(),
                    $"Expected exactly 1 attribute, got {context.Attributes.Length}."
                );
            }

            var attribute = context.Attributes[0];

            SmallList<Diagnostic>? warnings = null;
            var result = new SourceGeneratorConfiguration();
            try
            {
                foreach (var arg in attribute.NamedArguments)
                {
                    switch (arg.Key)
                    {
                        case nameof(SourceGeneratorConventionsAttribute.BrandedTypeNamePattern) when arg.Value.Value is string str:
                            result.BrandedTypeNamePattern = new Regex((string)arg.Value.Value, RegexOptions.Compiled);
                            break;

                        default:
                            SmallList.Push(ref warnings, Diagnostic.Create(
                                GeneratorDiagnosticDescriptors.UnsupportedConfiguration,
                                attribute.ApplicationSyntaxReference!.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span),
                                arg.Key
                            ));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                return Diagnostic.Create(
                    GeneratorDiagnosticDescriptors.InvalidConfiguration,
                    attribute.ApplicationSyntaxReference!.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span),
                    ex.Message
                );
            }
            return new Result<SourceGeneratorConfiguration>(result, warnings);
        }

        private static SmallList<Result<CustomAttributeSettings>>? ParseCustomAttributes(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            SmallList<Result<CustomAttributeSettings>>? result = null;
            foreach (var attribute in context.Attributes)
            {
                SmallList.Push(ref result, ParseCustomAttribute(attribute));
            }

            return result;
        }

        private static Result<CustomAttributeSettings> ParseCustomAttribute(AttributeData attribute)
        {
            try
            {
                var syntax = new StringBuilder();

                var type = (ITypeSymbol)attribute.ConstructorArguments[0].Value!;

                syntax.Append(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

                SmallList<Diagnostic>? warnings = null;
                TypedConstant constructorArguments = default;
                TypedConstant namedArguments = default;
                SmallList<string>? onlyForInnerTypes = null;
                SmallList<string>? exceptForInnerTypes = null;

                foreach (var arg in attribute.NamedArguments)
                {
                    switch (arg.Key)
                    {
                        case nameof(SourceGeneratorCustomAttributeAttribute.ConstructorArguments):
                            constructorArguments = arg.Value;
                            break;

                        case nameof(SourceGeneratorCustomAttributeAttribute.NamedArguments):
                            namedArguments = arg.Value;
                            break;

                        case nameof(SourceGeneratorCustomAttributeAttribute.OnlyForInnerTypes):
                            ParseTypeList(ref onlyForInnerTypes, arg.Value);
                            break;

                        case nameof(SourceGeneratorCustomAttributeAttribute.ExceptForInnerTypes):
                            ParseTypeList(ref exceptForInnerTypes, arg.Value);
                            break;

                        default:
                            SmallList.Push(ref warnings, Diagnostic.Create(
                                GeneratorDiagnosticDescriptors.UnsupportedConfiguration,
                                attribute.ApplicationSyntaxReference!.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span),
                                arg.Key
                            ));
                            break;
                    }
                }

                syntax.Append('(');

                var isFirst = true;

                if (!constructorArguments.IsNull)
                {
                    foreach (var constructorArg in constructorArguments.Values)
                    {
                        if (!isFirst)
                        {
                            syntax.Append(", ");
                        }
                        var diagnostic = WriteArgumentValue(syntax, constructorArg);
                        if (diagnostic is not null)
                        {
                            return Diagnostic.Create(
                                diagnostic,
                                attribute.ApplicationSyntaxReference!.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span)
                            );
                        }

                        isFirst = false;
                    }
                }

                if (!namedArguments.IsNull)
                {
                    if ((namedArguments.Values.Length % 2) != 0)
                    {
                        return Diagnostic.Create(
                            GeneratorDiagnosticDescriptors.InvalidNamedArgumentList,
                            attribute.ApplicationSyntaxReference!.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span)
                        );
                    }

                    for (var i = 0; i < namedArguments.Values.Length; i += 2)
                    {
                        var name = namedArguments.Values[i];
                        if (name.Type?.SpecialType != SpecialType.System_String)
                        {
                            return Diagnostic.Create(
                                GeneratorDiagnosticDescriptors.NamedArgumentNameMustBeString,
                                attribute.ApplicationSyntaxReference!.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span),
                                name.ToCSharpString()
                            );
                        }
                        var value = namedArguments.Values[i + 1];

                        if (!isFirst)
                        {
                            syntax.Append(", ");
                        }

                        syntax.Append(name.Value);
                        syntax.Append(" = ");

                        var diagnostic = WriteArgumentValue(syntax, value);
                        if (diagnostic is not null)
                        {
                            return Diagnostic.Create(
                                diagnostic,
                                attribute.ApplicationSyntaxReference!.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span)
                            );
                        }

                        isFirst = false;
                    }
                }

                syntax.Append(')');

                return new Result<CustomAttributeSettings>(
                        new CustomAttributeSettings(
                            syntax.ToString(),
                            onlyForInnerTypes,
                            exceptForInnerTypes
                        ),
                        warnings
                    );
            }
            catch (Exception ex)
            {
                return Diagnostic.Create(
                    GeneratorDiagnosticDescriptors.InvalidConfiguration,
                    attribute.ApplicationSyntaxReference!.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span),
                    ex.Message
                );
            }
        }

        private static bool IsEncodedGenericType(TypedConstant value, out INamedTypeSymbol? type, out DiagnosticDescriptor? diagnostic)
        {
            var isTypeArray = value.Type is not null
                && value.Type.TypeKind == TypeKind.Array
                && value.Type is IArrayTypeSymbol arrayType
                && arrayType.ElementType.ContainingNamespace.Name == "System"
                && arrayType.ElementType.Name == "Type";

            type = null;
            diagnostic = null;

            if (!isTypeArray)
            {
                return false;
            }

            var array = value.Values;

            if (array.Length == 0 || array[0].IsNull)
            {
                return false;
            }

            var openGeneric = (INamedTypeSymbol)array[0].Value!;
            if (!openGeneric.IsUnboundGenericType)
            {
                diagnostic = GeneratorDiagnosticDescriptors.InvalidGenericType;
                return false;
            }

            if (openGeneric.Arity != array.Length - 1)
            {
                diagnostic = GeneratorDiagnosticDescriptors.InvalidGenericType;
                return false;
            }

            var genericArguments = new ITypeSymbol[openGeneric.Arity];
            for (var i = 0; i < genericArguments.Length; ++i)
            {
                genericArguments[i] = (ITypeSymbol)array[i + 1].Value!;
            }

            type = openGeneric.ConstructedFrom.Construct(genericArguments);
            return true;
        }

        private static DiagnosticDescriptor? WriteArgumentValue(StringBuilder syntax, TypedConstant argument)
        {
            if (IsEncodedGenericType(argument, out var generic, out var diagnostic))
            {
                RenderTypeName(syntax, generic!);
            }
            else if (argument.Kind == TypedConstantKind.Type && argument.Value is INamedTypeSymbol type)
            {
                RenderTypeName(syntax, type);
            }
            else
            {
                syntax.Append(argument.ToCSharpString().Replace("{", "{{").Replace("}", "}}"));
            }
            return diagnostic;
        }

        private static void ParseTypeList(ref SmallList<string>? list, TypedConstant array)
        {
            foreach (var item in array.Values)
            {
                var type = (ITypeSymbol)item.Value!;
                SmallList.Push(ref list, type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }
        }

        private static void RenderTypeName(StringBuilder syntax, INamedTypeSymbol type)
        {
            if (type.ContainingNamespace.Name == "Branded")
            {
                if (type.Name == nameof(BrandedTypePlaceholder))
                {
                    syntax.Append("typeof({0})");
                    return;
                }

                if (type.Name == nameof(BrandedInnerTypePlaceholder))
                {
                    syntax.Append("typeof({1})");
                    return;
                }

                if (type.Name == nameof(BrandedConverterTypePlaceholder))
                {
                    syntax.Append("typeof({0}.Converter)");
                    return;
                }
            }

            syntax.Append("typeof(");
            if (type.IsGenericType)
            {
                syntax.Append(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Replace("global::Branded." + nameof(BrandedTypePlaceholder), "{0}")
                    .Replace("global::Branded." + nameof(BrandedInnerTypePlaceholder), "{1}")
                    .Replace("global::Branded." + nameof(BrandedConverterTypePlaceholder), "{0}.Converter")
                );
            }
            else
            {
                syntax.Append(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }
            syntax.Append(')');
        }

        private static bool IsBrandedTypeCandidate(SyntaxNode node, CancellationToken cancellationToken)
        {
            return node is RecordDeclarationSyntax recordDeclarationSyntax
                && recordDeclarationSyntax.ClassOrStructKeyword.Text == "struct"
                && recordDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));
        }

        private static Result<BrandedTypeCandidate> ParseBrandedTypeCandidate(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (symbol is not INamedTypeSymbol namedTypeSymbol)
            {
                return new(null, null);
            }

            var location = symbol.Locations.First();

            var recordDeclarationSyntax = (RecordDeclarationSyntax)context.Node;
            if (!recordDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return new Result<BrandedTypeCandidate>(
                    new BrandedTypeCandidate(
                        namedTypeSymbol.Name,
                        namedTypeSymbol.ContainingNamespace.ToDisplayString(),
                        namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        location,
                        "?",
                        "?",
                        null
                    ),
                    Diagnostic.Create(
                        GeneratorDiagnosticDescriptors.MissingPartialModifier,
                        location,
                        symbol.Name
                    )
                );
            }

            var ctor = namedTypeSymbol.GetMembers(".ctor")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(c => c.Parameters.Length == 1);

            if (ctor is null)
            {
                return new Result<BrandedTypeCandidate>(
                    new BrandedTypeCandidate(
                        namedTypeSymbol.Name,
                        namedTypeSymbol.ContainingNamespace.ToDisplayString(),
                        namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        location,
                        "?",
                        "?",
                        null
                    ),
                    Diagnostic.Create(
                        GeneratorDiagnosticDescriptors.MissingConstructor,
                        location,
                        symbol.Name
                    )
                );
            }

            var parameter = ctor.Parameters[0];

            SmallList<ImplementedInterface>? implementedInterfaces = null;
            foreach (var itf in parameter.Type.AllInterfaces)
            {
                var interfaceIsGeneric = itf.IsGenericType && itf.Arity == 1 && itf.TypeArguments[0].Equals(parameter.Type, SymbolEqualityComparer.Default);
                var lookupItf = interfaceIsGeneric ? itf.ConstructedFrom : itf;

                var lookupItfName = lookupItf.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (interfaceImplementationGenerators.ContainsKey(lookupItfName))
                {
                    var implementedType = interfaceIsGeneric ? lookupItf.Construct(namedTypeSymbol) : lookupItf;
                    SmallList.Push(ref implementedInterfaces, new ImplementedInterface(
                        lookupItfName,
                        implementedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    ));
                }
            }

            return new BrandedTypeCandidate(
                namedTypeSymbol.Name,
                namedTypeSymbol.ContainingNamespace.ToDisplayString(),
                namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                location,
                parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                parameter.Name,
                implementedInterfaces
            );
        }

        private sealed record BrandedTypeCandidate(
            string Name,
            string ContainingNamespace,
            string FullName,
            Location Location,
            string InnerTypeName,
            string PropertyName,
            SmallList<ImplementedInterface>? ImplementedInterfaces
        );

        private sealed record ImplementedInterface(
            string LookupName,
            string FullName
        );

        private sealed record Result<T>(T? Value, SmallList<Diagnostic>? Diagnostics = null)
        {
            public Result(SmallList<Diagnostic> Diagnostics) : this(default, Diagnostics) { }

            public static implicit operator Result<T>(Diagnostic diagnostic) => new(new SmallList<Diagnostic>(diagnostic));
            public static implicit operator Result<T>(T value) => new(value);
        }

        private static void GenerateBrandedType(
            SourceProductionContext context,
            BrandedTypeCandidate brandedType,
            SourceGeneratorConfiguration configuration,
            IEnumerable<CustomAttributeSettings> customAttributes,
            bool brandedAssemblyReferenced
        )
        {
            var source = GenerateSource(brandedType, configuration, customAttributes, brandedAssemblyReferenced);
            context.AddSource($"{brandedType.ContainingNamespace}_{brandedType.Name}.g.cs", source);

            // Not an error, just an informative message.
            context.ReportDiagnostic(Diagnostic.Create(
                GeneratorDiagnosticDescriptors.CodeGenerated,
                brandedType.Location,
                brandedType.Name
            ));
        }

        private delegate void InterfaceImplementationGeneratorDeletage(StringBuilder source, BrandedTypeCandidate targetType, ImplementedInterface itf);

        private static readonly Dictionary<string, InterfaceImplementationGeneratorDeletage> interfaceImplementationGenerators = new()
        {
            {
                "global::System.IComparable",
                (source, targetType, parameter) => source
                    .Append("    int global::System.IComparable.CompareTo(object other) => CompareTo((")
                    .Append(targetType.FullName)
                    .Append(")other);\n")
            },
            {
                "global::System.IFormattable",
                (source, targetType, parameter) => source
                    .Append("    public string ToString(string format, global::System.IFormatProvider formatProvider) => ")
                    .Append(targetType.PropertyName)
                    .Append(".ToString(format, formatProvider);\n")
            },
            {
                "global::System.IComparable<T>",
                (source, targetType, parameter) => source
                    .Append("    public int CompareTo(")
                    .Append(targetType.FullName)
                    .Append(" other) => ")
                    .Append(targetType.PropertyName)
                    .Append(".CompareTo(other.")
                    .Append(targetType.PropertyName)
                    .Append(");\n")
            }
        };

        private static string GenerateSource(
            BrandedTypeCandidate targetType,
            SourceGeneratorConfiguration configuration,
            IEnumerable<CustomAttributeSettings> customAttributes,
            bool brandedAssemblyReferenced)
        {
            var source = new StringBuilder();
            source.Append("// <auto-generated />\n");
            source.Append("#pragma warning disable 1591\n");

            source
                .Append("namespace ")
                .Append(targetType.ContainingNamespace)
                .Append(";\n");

            source.Append('\n');

            foreach (var customAttribute in customAttributes)
            {
                if (customAttribute.OnlyForInnerTypes is not null && !customAttribute.OnlyForInnerTypes.Contains(targetType.InnerTypeName))
                {
                    continue;
                }

                if (customAttribute.ExceptForInnerTypes is not null && customAttribute.ExceptForInnerTypes.Contains(targetType.InnerTypeName))
                {
                    continue;
                }

                source
                    .Append('[')
                    .AppendFormat(customAttribute.Template, targetType.FullName, targetType.InnerTypeName)
                    .Append("]\n");
            }

            source
                .Append("partial record struct ")
                .Append(targetType.Name)
                .Append('\n');

            var isFirstInterface = true;
            if (targetType.ImplementedInterfaces is not null)
            {
                foreach (var itf in targetType.ImplementedInterfaces)
                {
                    source
                        .Append(isFirstInterface ? "    : " : "    , ")
                        .Append(itf.FullName)
                        .Append('\n');

                    isFirstInterface = false;
                }
            }

            source.Append("{\n");

            if (brandedAssemblyReferenced)
            {
                source
                    .Append("    public sealed class Converter : global::Branded.IBrandedValueConverter<")
                    .Append(targetType.FullName)
                    .Append(", ")
                    .Append(targetType.InnerTypeName)
                    .Append(">\n");

                source.Append("    {\n");

                source
                    .Append("        public ")
                    .Append(targetType.FullName)
                    .Append(" Wrap(")
                    .Append(targetType.InnerTypeName)
                    .Append(" value) => new ")
                    .Append(targetType.FullName)
                    .Append("(value);\n");

                source
                    .Append("        public ")
                    .Append(targetType.InnerTypeName)
                    .Append(" Unwrap(")
                    .Append(targetType.FullName)
                    .Append(" value) => value.")
                    .Append(targetType.PropertyName)
                    .Append(";\n");

                source.Append("    }\n\n");

                source
                    .Append("    public static void Dispatch(global::Branded.IBrandedTypeDispatcher dispatcher) => dispatcher.Dispatch<")
                    .Append(targetType.FullName)
                    .Append(", ")
                    .Append(targetType.InnerTypeName)
                    .Append(", ")
                    .Append(targetType.FullName)
                    .Append(".Converter>();\n\n");
            }

            source
                .Append("    public override string ToString() => ")
                .Append(targetType.PropertyName)
                .Append(".ToString();\n\n");

            source
                .Append("    public static implicit operator ")
                .Append(targetType.InnerTypeName)
                .Append('(')
                .Append(targetType.FullName)
                .Append(" value) => value.")
                .Append(targetType.PropertyName)
                .Append(";\n");

            if (targetType.ImplementedInterfaces is not null)
            {
                foreach (var itf in targetType.ImplementedInterfaces)
                {
                    source.Append('\n');
                    interfaceImplementationGenerators[itf.LookupName](source, targetType, itf);
                }
            }

            source.Append("}");

            //Debugger.Launch();

            return source.ToString();
        }
    }

    public class GeneratorDiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor CodeGenerated = new(
            id: "BRND000",
            title: "Branded type code generated",
            messageFormat: "Branded type code generated for {0}",
            category: "Usage",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingPartialModifier = new(
            id: "BRND001",
            title: "Missing 'partial' modifier",
            messageFormat: "{0} must be partial",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingConstructor = new(
            id: "BRND002",
            title: "Missing constructor",
            messageFormat: "{0} must have a constructor taking a single parameter",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidConfiguration = new(
            id: "BRND003",
            title: "Invalid configuration",
            messageFormat: "The configuration is invalid because {0}",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnsupportedConfiguration = new(
            id: "BRND004",
            title: "Unsupported configuration",
            messageFormat: "{0} is not supported, most probably due to a mismatch between Branded.SourceGenerator and Branded",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidGenericType = new(
            id: "BRND005",
            title: "Invalid generic argument list",
            messageFormat: "When using an Type[], the first element must be an unbound generic type, and the remainder of the array must the list of generic argument types",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidNamedArgumentList = new(
            id: "BRND006",
            title: "Invalid named argument list",
            messageFormat: "NamedArguments must consist of pairs of property names (strings) and their value",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NamedArgumentNameMustBeString = new(
            id: "BRND007",
            title: "Invalid named argument list",
            messageFormat: "The named argument property name '{0}' is not a string",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
