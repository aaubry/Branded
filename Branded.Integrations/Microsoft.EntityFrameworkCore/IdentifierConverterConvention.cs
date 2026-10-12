// MIT License
// 
// Copyright (C) 2025, 2026 Antoine Aubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#nullable enable

namespace Branded.Integrations.Microsoft.EntityFrameworkCore
{
    public sealed class IdentifierConverterConvention
        : global::Microsoft.EntityFrameworkCore.Metadata.Conventions.IEntityTypeAddedConvention
    {
        public void ProcessEntityTypeAdded(
            global::Microsoft.EntityFrameworkCore.Metadata.Builders.IConventionEntityTypeBuilder entityTypeBuilder,
            global::Microsoft.EntityFrameworkCore.Metadata.Conventions.IConventionContext<global::Microsoft.EntityFrameworkCore.Metadata.Builders.IConventionEntityTypeBuilder> context)
        {
            var clrType = entityTypeBuilder.Metadata.ClrType;

            foreach (var propertyInfo in clrType.GetProperties(global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance))
            {
                var propertyClrType = global::System.Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType; // unwrap Nullable<UserIdentifier>

                var converterType = propertyClrType.GetNestedType("Converter");
                if (converterType is null)
                {
                    continue;
                }

                var brandedValueConverterImpl = global::System.Linq.Enumerable.FirstOrDefault(
                    converterType.GetInterfaces(),
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(global::Branded.IBrandedValueConverter<,>)
                );

                if (brandedValueConverterImpl is null)
                {
                    continue;
                }

                var args = brandedValueConverterImpl.GetGenericArguments();
                var identifierConverterType = typeof(IdentifierConverter<,,>)
                    .MakeGenericType(args[0], args[1], converterType);

                var converter = (global::Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter)global::System.Activator.CreateInstance(identifierConverterType)!;

                // Explicitly claim the property: EF's own discovery would otherwise skip it,
                // since it doesn't recognize the branded identifier type as a primitive.
                entityTypeBuilder.Property(propertyInfo)!.HasConversion(converter);
            }
        }

        private sealed class IdentifierConverter<TIdentifier, TInner, TConverter>
            : global::Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<TIdentifier, TInner>
            where TConverter : IBrandedValueConverter<TIdentifier, TInner>, new()
        {
            private static readonly TConverter Converter = new TConverter();

            public IdentifierConverter()
                : base(id => Converter.Unwrap(id), val => Converter.Wrap(val))
            {
            }
        }
    }
}