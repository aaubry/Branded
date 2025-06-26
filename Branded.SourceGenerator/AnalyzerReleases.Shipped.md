; Shipped analyzer releases
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
BRND000 | Usage    | Info     | Notifies that the branded type has been correctly generated.
BRND001 | Usage    | Error    | A branded type has been identified but is missing the partial modifier.
BRND002 | Usage    | Error    | A branded type must have a constructor taking a single parameter.
BRND003 | Usage    | Error    | A configuration parameter is invalid.
BRND004 | Usage    | Warning  | A configuration parameter is not supported.
BRND005 | Usage    | Error    | When using an Type[], the first element must be an unbound generic type, and the remainder of the array must the list of generic argument types.
BRND006 | Usage    | Error    | NamedArguments must consist of pairs of property names (strings) and their value.
BRND007 | Usage    | Error    | Some named argument property name is not a string.
